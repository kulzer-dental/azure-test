using Newtonsoft.Json;

namespace KDC.Main.Models
{
    public class StoreConfiguration
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        [JsonProperty("website_id")]
        public int WebsiteId { get; set; }
        public string Locale { get; set; } = string.Empty;

        [JsonProperty("base_currency_code")]
        public string BaseCurrencyCode { get; set; } = string.Empty;

        [JsonProperty("default_display_currency_code")]
        public string DefaultDisplayCurrencyCode { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;

        [JsonProperty("weight_unit")]
        public string WeightUnit { get; set; } = string.Empty;

        [JsonProperty("base_url")]
        public string BaseUrl { get; set; } = string.Empty;

        [JsonProperty("base_link_url")]
        public string BaseLinkUrl { get; set; } = string.Empty;

        [JsonProperty("base_static_url")]
        public string BaseStaticUrl { get; set; } = string.Empty;

        [JsonProperty("base_media_url")]
        public string BaseMediaUrl { get; set; } = string.Empty;

        [JsonProperty("secure_base_url")]
        public string SecureBaseUrl { get; set; } = string.Empty;

        [JsonProperty("secure_base_link_url")]
        public string SecureBaseLinkUrl { get; set; } = string.Empty;

        [JsonProperty("secure_base_static_url")]
        public string SecureBaseStaticUrl { get; set; } = string.Empty;

        [JsonProperty("secure_base_media_url")]
        public string SecureBaseMediaUrl { get; set; } = string.Empty;

        [JsonProperty("extension_attributes")]
        public ExtensionAttributes ExtensionAttributes { get; set; } = new ExtensionAttributes();
    }
}
