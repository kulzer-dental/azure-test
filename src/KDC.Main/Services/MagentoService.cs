using Duende.IdentityServer.EntityFramework.DbContexts;
using KDC.Main.Config;
using KDC.Main.Helpers;
using KDC.Main.Models;
using KDC.Main.Models.Magento;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace KDC.Main.Services
{
    public class MagentoService : IMagentoService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiEndpoints _api;
        private readonly IClientConfigurationRepository _clientConfigurationRepository;
        private readonly MagentoApiAuthentication _magentoApiAuthentication;
        private readonly ILogger<MagentoService> _logger;

        public MagentoService(HttpClient httpClient, IOptions<ApiEndpoints> apiEndpoints,
            IOptions<MagentoApiAuthentication> magentoApiAuthentication,
            IClientConfigurationRepository clientConfigurationRepository,
            ILogger<MagentoService> logger)
        {
            _httpClient = httpClient;
            _api = apiEndpoints.Value;
            _magentoApiAuthentication = magentoApiAuthentication.Value;
            _clientConfigurationRepository = clientConfigurationRepository;
            _logger = logger;
        }

        public async Task<MagentoAuthResult> AuthenticateAsync(string returnUrl, string email, string password)
        {
            try
            {
                bool existRedirectUriInDB = false;
                var redirect_uri = UrlHelper.ExtractRedirectUri(returnUrl, "redirect_uri");

                if (redirect_uri == null)
                    return new MagentoAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Could not retrieve redirect_uri query param"
                    };

                existRedirectUriInDB = await _clientConfigurationRepository.IsValidRedirectUri(redirect_uri);

                if (existRedirectUriInDB == false)
                    return new MagentoAuthResult
                    {
                        Success = false,
                        ErrorMessage = "redirect_uri is not valid"
                    };

                var uri = new Uri(redirect_uri);
                var baseUrl = uri.GetLeftPart(UriPartial.Authority) + _api.Login;
                var requestUrl = BuildStoreSpecificUrl(baseUrl, returnUrl);

                if (string.IsNullOrWhiteSpace(requestUrl))
                    return new MagentoAuthResult
                    {
                        Success = false,
                        ErrorMessage = "Store code is not specified in the return url"
                    };

                var response = await _httpClient.PostAsJsonAsync(requestUrl, new { username = email, password });

                if (response.IsSuccessStatusCode)
                {
                    return new MagentoAuthResult
                    {
                        Success = true,
                        Token = await response.Content.ReadAsStringAsync()
                    };
                }

                return new MagentoAuthResult
                {
                    Success = false,
                    ErrorMessage = "Invalid credentials"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Magento authentication for email {Email}", email);
                return new MagentoAuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication service unavailable"
                };
            }
        }

        public async Task<MagentoDataResult<StoreConfiguration>?> GetStoreConfigAsync(string? returnUrl = null, string? storeCode = null)
        {
            try
            {
                var redirect_uri = UrlHelper.ExtractRedirectUri(returnUrl, "redirect_uri");
                List<string> baseURLs = new List<string>();

                if (redirect_uri == null)
                {
                    var redirectUris = await _clientConfigurationRepository.GetRedirectUrisForClientAsync("m2shop-belvg");
                    baseURLs = redirectUris
                        .Select(r => new Uri(r))
                        .Select(uri => $"{uri.Scheme}://{uri.Host}")
                        .ToList();

                    if (!baseURLs.Any())
                    {
                        return new MagentoDataResult<StoreConfiguration>
                        {
                            Success = false,
                            ErrorMessage = "No base URLs found in configuration"
                        };
                    }

                    return await TryMultipleStoreConfigRequests(baseURLs, returnUrl, storeCode);
                }
                else
                {
                    var uri = new Uri(redirect_uri);
                    var baseUrl = uri.GetLeftPart(UriPartial.Authority) + _api.StoreConfig;
                    var requestUrl = BuildStoreSpecificUrl(baseUrl, returnUrl);

                    if (string.IsNullOrWhiteSpace(requestUrl))
                        return new MagentoDataResult<StoreConfiguration>
                        {
                            Success = false,
                            ErrorMessage = "Store code is not specified in the return url"
                        };

                    return await MakeStoreConfigRequest(requestUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving store configuration");
                return (new MagentoDataResult<StoreConfiguration>
                {
                    Success = false,
                    ErrorMessage = "Store configuration service unavailable"
                });
            }
        }

        public async Task<MagentoRequestResultBase> CreateMagentoAccount(string returnUrl, MagentoAccount magentoAccount)
        {
            try
            {
                var redirect_uri = UrlHelper.ExtractRedirectUri(returnUrl, "redirect_uri");

                if (redirect_uri == null)
                    return new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "No redirect uri param found"
                    };

                var uri = new Uri(redirect_uri);
                var baseUrl = uri.GetLeftPart(UriPartial.Authority) + _api.CreateUser;
                var requestUrl = BuildStoreSpecificUrl(baseUrl, returnUrl);

                if (string.IsNullOrWhiteSpace(requestUrl))
                    return new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "Store code is not specified in the return url"
                    };

                var json = JsonConvert.SerializeObject(magentoAccount);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    var storesConfiguration = JsonConvert.DeserializeObject<object>(json);

                    return (new MagentoRequestResultBase
                    {
                        Success = true,
                    });
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                if (responseBody.Contains("A customer with the same email address already exists in an associated website."))
                {
                    return (new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "An account with this email adress already exists!"
                    });
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a new user on Magento store for email {Email}", magentoAccount?.Customer?.Email);
                return (new MagentoRequestResultBase
                {
                    Success = false,
                    ErrorMessage = "Error while created a new user on Magento store"
                });
            }

            return (new MagentoRequestResultBase
            {
                Success = false,
                ErrorMessage = "The user could not be created"
            });
        }

        public async Task<MagentoRequestResultBase> InvalidateSession(string? returnUrl, string email, string? storeCode = null)
        {
            try
            {
                string? redirect_uri = null;

                if (UrlHelper.IsRedirectUrlNullOrDefault(returnUrl) == false)
                {
                    redirect_uri = UrlHelper.ExtractRedirectUri(returnUrl, "redirect_uri");
                }

                if (string.IsNullOrWhiteSpace(redirect_uri) == true)
                {
                    if (string.IsNullOrWhiteSpace(storeCode) == false)
                    {
                        var storeConfiguration = await GetStoreConfigurationByCodeAsync(storeCode);
                        if (storeConfiguration == null)
                        {
                            _logger.LogError($"Error while attempting to invalidate session for user {email} " +
                            $"Store configuration for code {storeCode} not found");

                            return new MagentoRequestResultBase()
                            {
                                Success = false,
                                ErrorMessage = $"Store configuration for code {storeCode} not found"
                            };
                        }
                        redirect_uri = storeConfiguration.BaseUrl;
                    }
                    else
                    {
                        _logger.LogError($"Error while attempting to invalidate session for user {email} " +
                        $"No store code for this");

                        return new MagentoRequestResultBase()
                        {
                            Success = false,
                            ErrorMessage = $"No storecode provided for this"
                        };
                    }
                }

                var uri = new Uri(redirect_uri);
                var baseUrl = uri.GetLeftPart(UriPartial.Authority) + _api.InvalidateSession;
                var requestUrl = BuildStoreSpecificUrl(baseUrl, returnUrl, storeCode);

                if (string.IsNullOrWhiteSpace(requestUrl))
                {
                    _logger.LogError($"Error while attempting to invalidate session for user {email} " +
                        $"request url is null or empty requestUrl: {requestUrl}");

                    return new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "Store code is not specified in the return url"
                    };
                }

                var requestData = new { email = email };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
                request.Content = content;
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                   _magentoApiAuthentication.TokenType,
                   _magentoApiAuthentication.AccessToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();
                    bool hasSessionBeenInvalidated = JsonConvert.DeserializeObject<bool>(json);
                    return (new MagentoRequestResultBase
                    {
                        Success = hasSessionBeenInvalidated,
                    });
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogError($"Error while attempting to invalidate session for user {email} status code for calling invalidate session:   " +
                    $"{response?.IsSuccessStatusCode}, response body:  {responseBody}");

                return (new MagentoRequestResultBase
                {
                    Success = false,
                    ErrorMessage = $"Could not invalidate the session for {email}, responseBody"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while attempting to invalidate session for user {Email} stack trace: {stacktrace}", email, ex.StackTrace);
                _logger.LogError($"Error while attempting to invalidate session for user {email} exeption {ex} stack trace: {ex.StackTrace}");

                return (new MagentoRequestResultBase
                {
                    Success = false,
                    ErrorMessage = $"Error while attempting to invalidate session for user {email}"
                });
            }
        }

        public async Task<MagentoRequestResultBase> IsEmailRegisteredAsync(string returnUrl, string email)
        {
            try
            {
                var redirect_uri = UrlHelper.ExtractRedirectUri(returnUrl, "redirect_uri");

                if (redirect_uri == null)
                    return new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "No redirect uri param found"
                    };

                var uri = new Uri(redirect_uri);
                var baseUrl = uri.GetLeftPart(UriPartial.Authority) + _api.CustomerSearch;
                var requestUrl = BuildStoreSpecificUrl(baseUrl, returnUrl);

                if (string.IsNullOrWhiteSpace(requestUrl))
                    return new MagentoRequestResultBase
                    {
                        Success = false,
                        ErrorMessage = "Store code is not specified in the return url"
                    };

                var encodedEmail = Uri.EscapeDataString(email);

                var searchParams = $"searchCriteria[filter_groups][0][filters][0][field]=email" +
                       $"&searchCriteria[filter_groups][0][filters][0][value]={encodedEmail}" +
                       $"&searchCriteria[filter_groups][0][filters][0][condition_type]=eq";

                var fullUrl = $"{requestUrl}?{searchParams}";

                using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                   _magentoApiAuthentication.TokenType,
                   _magentoApiAuthentication.AccessToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var magentoCustomerSearchResult = JsonConvert.DeserializeObject<MagentoCustomerSearchResult>(json);

                    if (magentoCustomerSearchResult?.TotalCount > 0)
                    {
                        return (new MagentoRequestResultBase
                        {
                            Success = true,
                        });
                    }
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                return (new MagentoRequestResultBase
                {
                    Success = false,
                    ErrorMessage = $"Could not find in Magento {email}, responseBody: {responseBody}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while attempting to find if user exists in Magento Store for email {Email}", email);
                return (new MagentoRequestResultBase
                {
                    Success = false,
                    ErrorMessage = $"Error while attempting to find if user exist in Magento Store {email}"
                });
            }
        }

        public async Task<string?> GetAuthorizationRequest(string storeCode)
        {
            try
            {
                var storeConfiguration = await GetStoreConfigurationByCodeAsync(storeCode);

                if (storeConfiguration == null)
                {
                    return null;
                }

                var uri = new Uri(storeConfiguration.BaseUrl);
                StringBuilder authUrl = new StringBuilder(uri.GetLeftPart(UriPartial.Authority));

                authUrl.Append(_api.AuthorizationRequest);
                authUrl.Replace("{store_base_url}", storeConfiguration.BaseUrl);
                authUrl.Replace("{app_name}", storeConfiguration.ExtensionAttributes.IdentityServerAppName);

                return authUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while building authorization request URL for store code {StoreCode}", storeCode);
                return null;
            }
        }

        private string? BuildStoreSpecificUrl(string baseUrl, string? returnUrl = null, string? storeCode = null)
        {
            if (string.IsNullOrWhiteSpace(storeCode) == true && string.IsNullOrWhiteSpace(returnUrl) == false)
            {
                storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
            }

            if (string.IsNullOrWhiteSpace(storeCode))
            {
                return null;
            }

            if (baseUrl.Contains("{store_code}"))
            {
                return baseUrl.Replace("{store_code}", storeCode);
            }

            if (baseUrl.Contains("/rest/") && !baseUrl.Contains($"/rest/{storeCode}/"))
            {
                return baseUrl.Replace("/rest/", $"/rest/{storeCode}/");
            }

            return baseUrl;
        }

        private async Task<MagentoDataResult<StoreConfiguration>> TryMultipleStoreConfigRequests(List<string> baseURLs, string? returnUrl, string? storeCode = null)
        {
            var errors = new List<string>();

            foreach (var baseURL in baseURLs)
            {
                try
                {
                    var fullUrl = baseURL + _api.StoreConfig;
                    var requestUrl = BuildStoreSpecificUrl(fullUrl, returnUrl, storeCode);

                    if (string.IsNullOrWhiteSpace(requestUrl))
                    {
                        errors.Add($"Could not build store-specific URL for {baseURL}");
                        continue;
                    }

                    var result = await MakeStoreConfigRequest(requestUrl);

                    if (result.Success)
                    {
                        return result; // Return on first success
                    }

                    errors.Add($"{baseURL}: {result.ErrorMessage}");
                }
                catch (TaskCanceledException ex)
                {
                    errors.Add($"{baseURL}: Request timed out  exeption {ex.Message}");
                    // Continue to next URL on timeout
                }
                catch (HttpRequestException ex)
                {
                    errors.Add($"{baseURL}: HTTP error - {ex.Message}");
                    // Continue to next URL on HTTP error
                }
                catch (Exception ex)
                {
                    errors.Add($"{baseURL}: Unexpected error - {ex.Message}");
                    // Continue to next URL on any other error
                }
            }

            // All requests failed
            return new MagentoDataResult<StoreConfiguration>
            {
                Success = false,
                ErrorMessage = $"All store configuration requests failed. Errors: {string.Join("; ", errors)}"
            };
        }

        private async Task<MagentoDataResult<StoreConfiguration>> MakeStoreConfigRequest(string requestUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var storesConfiguration = JsonConvert.DeserializeObject<IReadOnlyList<StoreConfiguration>>(json);

                    return new MagentoDataResult<StoreConfiguration>
                    {
                        Success = true,
                        Data = storesConfiguration
                    };
                }

                return new MagentoDataResult<StoreConfiguration>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to {RequestUrl} timed out", requestUrl);
                return new MagentoDataResult<StoreConfiguration>
                {
                    Success = false,
                    ErrorMessage = $"Request to {requestUrl} timed out after {_httpClient.Timeout.TotalSeconds} seconds"
                };
            }
        }

        public async Task<StoreConfiguration?> GetStoreConfigurationByCodeAsync(string storeCode)
        {
            var storesConfiguration = await GetStoreConfigAsync(storeCode: storeCode);
            return storesConfiguration?.Data?.FirstOrDefault(s => s.Code == storeCode);
        }
    }
}
