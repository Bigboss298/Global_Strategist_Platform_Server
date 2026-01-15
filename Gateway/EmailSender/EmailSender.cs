using SendGrid;
using SendGrid.Helpers.Mail;

namespace Global_Strategist_Platform_Server.Gateway.EmailSender
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public SendGridEmailService(IConfiguration configuration)
        {
            var config = configuration.GetSection("SendGrid");
            _apiKey = config["ApiKey"];
            _senderEmail = config["SenderEmail"];
            _senderName = config["SenderName"];
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_senderEmail, _senderName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.IsSuccessStatusCode;
        }
    }
}
