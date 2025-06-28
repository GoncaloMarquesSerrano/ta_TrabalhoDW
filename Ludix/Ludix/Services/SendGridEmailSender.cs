namespace Ludix.Services
{
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.Extensions.Options;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Net.Mail;

    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridSettings _settings;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(IOptions<SendGridSettings> settings, ILogger<SendGridEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var client = new SendGridClient(_settings.ApiKey);
                var from = new EmailAddress(_settings.FromEmail, _settings.FromName);
                var to = new EmailAddress(email);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Não foi possível enviar o email. Status: {StatusCode}, Body: {Body}",
                        response.StatusCode, responseBody);
                    throw new Exception($"Não foi possível enviar o email: {response.StatusCode}");
                }

                _logger.LogInformation("Email enviado para {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro a enviar email para {Email}", email);
                throw;
            }
        }
    }
}
