namespace KDC.Main.Config {

    /// <summary>
    /// Represents the SMTP config from environment
    /// </summary>
    public class SmtpConfig
    {
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }

}