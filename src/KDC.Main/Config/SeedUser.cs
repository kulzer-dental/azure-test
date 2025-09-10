namespace KDC.Main.Config
{

    /// <summary>
    /// Model class for initial seeding of users
    /// </summary>
    public class SeedUser
    {
        /// <summary>
        /// Login username, will also be set as email!
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Cleartext password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string? GivenName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string? FamilyName { get; set; }

        /// <summary>
        /// AppRoles that this user should have
        /// Must match AppRoles.cs
        /// </summary>
        public string[]? Roles { get; set; }

        /// <summary>
        /// Preferred language of the user in iso format
        /// </summary>
        public string? Culture { get; set; }
    }

}
