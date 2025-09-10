using System.Security.Policy;

namespace KDC.Main.Helpers
{
    public static class UrlHelper
    {
        public static string? ExtractRedirectUri(string? returnUrl, string queryParam)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(queryParam))
                return null;

            try
            {
                var uriToUse = returnUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? returnUrl
                    : "https://example.com" + returnUrl;

                var query = new UriBuilder(uriToUse).Query;
                var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query);

                return queryParams.TryGetValue(queryParam, out var value)
                    ? value.FirstOrDefault()
                    : null;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        public static bool IsRedirectUrlNullOrDefault(string? returnUrl)
        {
            if (returnUrl == null || 
                (returnUrl != null && 
                  (returnUrl.Equals("/", StringComparison.OrdinalIgnoreCase) 
                  || returnUrl.Equals("~/", StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }

            return false;
        }
    }
}
