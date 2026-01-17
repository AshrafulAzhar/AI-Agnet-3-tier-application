using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;
using UserManagement.Domain.Interfaces;
using UserManagement.Service.Settings;

namespace UserManagement.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Welcome to Our Platform!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h1>Welcome, {userName}!</h1>
                    <p>Thank you for registering with us. We are excited to have you on board!</p>
                    <p>If you have any questions, feel free to reply to this email.</p>
                    <br/>
                    <p>Best regards,<br/>The Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, 
                    _emailSettings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                Console.WriteLine($"[EMAIL] Welcome email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                // In a production environment, we might want to log this and not fail the registration, 
                // or use a background worker/queue.
                Console.WriteLine($"[ERROR] Failed to send email to {toEmail}: {ex.Message}");
            }
        }
    }
}
