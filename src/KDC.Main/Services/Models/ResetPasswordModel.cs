namespace KDC.Main.Services.Models
{
    public class ResetPasswordModel
    {
        public string ResetUrl { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
