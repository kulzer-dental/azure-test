using Newtonsoft.Json;

namespace KDC.Main.Models
{
    public class ExtensionAttributes
    {
        [JsonProperty("identity_server_app_name")]
        public string IdentityServerAppName { get; set; } = string.Empty;

        [JsonProperty("enforce_identity_server")]
        public int EnforceIdentityServer { get; set; }

        [JsonProperty("email_confirmation_enabled")]
        public int EmailConfirmationEnabled { get; set; }

        [JsonProperty("self_service_registration_external_url")]
        public string SelfServiceRegistrationExternalUrl { get; set; } = string.Empty;

        [JsonProperty("self_service_registration_enabled")]
        public int SelfServiceRegistrationEnabled { get; set; }
    }
}