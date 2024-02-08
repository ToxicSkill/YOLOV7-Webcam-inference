namespace YoloV7WebCamInference.Models
{
    public class EMail(string subject, string message)
    {
        public string Sender { get; set; } = Properties.Settings.Default.Sender;

        public string Subject { get; set; } = subject;

        public string Message { get; set; } = message;

        public string Recipient { get; set; } = Properties.Settings.Default.Recipient;

        public string SenderPassword { get; set; } = Properties.Settings.Default.SenderPassword;

        public int SenderSmtpPort { get; set; } = Properties.Settings.Default.SenderSmtpPort;

        public string SmptServer { get; set; } = Properties.Settings.Default.SmptServer;
    }
}
