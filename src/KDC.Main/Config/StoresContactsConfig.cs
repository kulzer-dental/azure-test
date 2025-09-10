namespace KDC.Main.Config;

public class StoresContactsConfig
{
    public Dictionary<string, StoreContact> Stores { get; set; } = new();
}

public class StoreContact
{
    public string Id { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string ContactPageUrl { get; set; } = string.Empty;
    public string PrivacyPolicyUrl { get; set; } = string.Empty;
}