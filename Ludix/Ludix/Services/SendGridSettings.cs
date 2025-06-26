namespace Ludix.Services
{
    public class SendGridSettings
    {
        /// <summary>
        /// Chave da API do SendGrid
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        /// <summary>
        /// Email do remetente
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;
        /// <summary>
        /// Nome do remetente
        /// </summary>
        public string FromName { get; set; } = string.Empty;
    }
}
