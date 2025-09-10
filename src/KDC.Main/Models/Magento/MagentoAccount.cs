using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace KDC.Main.Models.Magento
{
    public class MagentoAccount
    {
        [JsonProperty("customer")]
        public CustomerInfo Customer { get; set; } = new();

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        // Nested class for the customer object
        public class CustomerInfo
        {
            [JsonProperty("lastname")]
            public string LastName { get; set; } = string.Empty;

            [JsonProperty("firstname")]
            public string FirstName { get; set; } = string.Empty;

            [JsonProperty("email")]
            public string Email { get; set; } = string.Empty;
        }
    }
}
