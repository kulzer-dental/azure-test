namespace KDC.Main.Config {

public class ExternalAuthConfig {
    public required string Provider { get; set; }
    public bool Enabled { get; set; }
    public required string ClientId { get; set; }
    public  required string ClientSecret { get; set; }
}

}