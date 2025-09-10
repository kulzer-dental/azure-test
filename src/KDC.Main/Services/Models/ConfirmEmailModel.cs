namespace KDC.Main.Services.Models
{
    public class ConfirmEmailModel
    {
        public string ConfirmUrl { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}