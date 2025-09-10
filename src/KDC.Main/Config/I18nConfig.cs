namespace KDC.Main.Config;

public class I18nConfig
{
    public required string DefaultCulture { get; set; } = "en";
    public required string[] SupportedCultures { get; set; }
}