using Newtonsoft.Json;

namespace KDC.Main.Models.Magento
{
    public class MagentoCustomerSearchResult
    {
        [JsonProperty("items")]
        public List<MagentoCustomer> Items { get; set; } = new();

        [JsonProperty("search_criteria")]
        public object? SearchCriteria { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }
}