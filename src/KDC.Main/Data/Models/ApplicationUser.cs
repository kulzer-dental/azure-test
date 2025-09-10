using Microsoft.AspNetCore.Identity;

namespace KDC.Main.Data.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public required string StoreCode { get; set; }
}

