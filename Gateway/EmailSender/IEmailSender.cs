namespace Global_Strategist_Platform_Server.Gateway.EmailSender
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent);
    }
}
