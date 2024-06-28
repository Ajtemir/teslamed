using MimeKit;
using MailKit;
using MailKit.Security;
using System.Threading.Tasks;

namespace TeslaMed.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Администрация сайта ТЕСЛАМЕД", "teslamed@inbox.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("teslamed@inbox.ru", "YdfL1fUjHDpCR1nRs48k");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}