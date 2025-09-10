namespace KDC.Main.Config {
    public class CorsConfig
    {
        public static string PolicyName = "AllowSpecificOrigins";

        public required string[] AllowedOrigins { get; set; }
    }
}