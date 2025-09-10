using Newtonsoft.Json;

namespace KDC.Main.Models.Magento
{
    public class MagentoCustomer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;

        [JsonProperty("created_in")]
        public string CreatedIn { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [JsonProperty("lastname")]
        public string LastName { get; set; } = string.Empty;

        [JsonProperty("gender")]
        public int Gender { get; set; }

        [JsonProperty("store_id")]
        public int StoreId { get; set; }

        [JsonProperty("website_id")]
        public int WebsiteId { get; set; }

        [JsonProperty("disable_auto_group_change")]
        public int DisableAutoGroupChange { get; set; }
    }
}