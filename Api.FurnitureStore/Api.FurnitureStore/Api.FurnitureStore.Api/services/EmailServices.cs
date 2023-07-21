using MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Api.FurnitureStore.Api.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Api.FurnitureStore.Api.services
{
    public class EmailServices : IEmailSender
    {
        private readonly SmtpSettings _smtpSetting;

        public EmailServices(IOptions<SmtpSettings> smtpSetting)
        {
            _smtpSetting = smtpSetting.Value;
        }
        public async Task SendEmailAsync(string email , string subject , string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_smtpSetting.SenderName , _smtpSetting.SenderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlMessage };

                using (var client = new SmtpClient ())
                {
                    await client.ConnectAsync( _smtpSetting.Server);
                    await client.AuthenticateAsync(_smtpSetting.UserName , _smtpSetting.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch( Exception)
            {
                throw;
            }
        }
    }
}
