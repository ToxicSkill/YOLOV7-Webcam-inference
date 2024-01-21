using System;
using System.Net;
using System.Net.Mail;
using YoloV7WebCamInference.Interfaces;
using YoloV7WebCamInference.Models;

namespace YoloV7WebCamInference.NotificationService
{
    public class EMailService : IEmailService
    {
        public void SendMail(EMail email)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(email.Sender)
            };
            mail.To.Add(new MailAddress(email.Recipient));
            mail.Subject = email.Subject;
            mail.Body = email.Message;

            var smtpClient = new SmtpClient(email.SmptServer)
            {
                Port = email.SenderSmtpPort,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email.Sender, email.SenderPassword),
                EnableSsl = true
            };

            try
            {
                smtpClient.Send(mail);
                Console.WriteLine($"[INFO] Mail has been sended to {email.Recipient}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Mail sending failed to {email.Recipient} - {ex.Message}");
            }

            smtpClient.Dispose();
        }
    }
}
