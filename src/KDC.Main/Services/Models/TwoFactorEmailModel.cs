namespace KDC.Main.Services.Models
{
    public class TwoFactorEmailModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public string TwoFactorCode { get; set; } = string.Empty;
    }
}
