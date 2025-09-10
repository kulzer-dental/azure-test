namespace KDC.Main.Models;

/// <summary>
/// Profile of a user account
/// </summary>
public class UserProfileViewModel {
    /// <summary>
    /// First name
    /// </summary>
    public string? GivenName { get; set; }
    /// <summary>
    /// Last name
    /// </summary>
    public string? FamilyName { get; set; }
    /// <summary>
    /// Combination of first name and last name
    /// </summary>
    public string? DisplayName { get; set; }
    /// <summary>
    /// Language and optional culture information. Either two letter iso code (en) or language-COUNTRY (en-GB) 
    /// </summary>
    public string? PreferredCulture { get; set; }
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
}