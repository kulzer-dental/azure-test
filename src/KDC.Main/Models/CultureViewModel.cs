namespace KDC.Main.Models;

/// <summary>
/// Represents an application culture for i18n
/// </summary>
public class CultureViewModel {
    /// <summary>
    /// Flags if this is the system default language
    /// </summary>
    public bool IsSystemDefault { get; set; }
    /// <summary>
    /// English name of the culture
    /// </summary>
    public required string EnglishName { get; set; }
    /// <summary>
    /// Name of the culture in it's language
    /// </summary>
    public required string NativeName { get; set; }
    /// <summary>
    /// Either two digit language code or language-COUNTRY combination like en-GB
    /// </summary>
    public required string IsoCode { get; set; }
}