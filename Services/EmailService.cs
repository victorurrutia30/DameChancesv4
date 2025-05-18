using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace DameChanceSV2.Services
{
    // ==========================================
    // INTERFAZ IEmailService
    // Define el contrato para enviar correos electrónicos
    // ==========================================
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body);
    }

    // ==========================================
    // IMPLEMENTACIÓN DE IEmailService
    // Usa SMTP para enviar correos electrónicos HTML
    // Los datos se leen desde appsettings.json (MailSettings)
    // ==========================================
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        // Constructor con inyección de configuración
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ==========================================
        // MÉTODO: SendEmail
        // ENVÍA UN CORREO USANDO SMTP
        // ==========================================
        public void SendEmail(string toEmail, string subject, string body)
        {
            // Leer la sección "MailSettings" del archivo de configuración
            var mailSettings = _configuration.GetSection("MailSettings");

            // Obtener los valores individuales del archivo de configuración
            var fromEmail = mailSettings.GetValue<string>("FromEmail");     // Correo del remitente
            var displayName = mailSettings.GetValue<string>("DisplayName"); // Nombre del remitente
            var username = mailSettings.GetValue<string>("Username");       // Nombre de usuario SMTP
            var password = mailSettings.GetValue<string>("Password");       // Contrasena SMTP
            var host = mailSettings.GetValue<string>("Host");               // Servidor SMTP
            var port = mailSettings.GetValue<int>("Port");                  // Puerto SMTP

            // Crear cliente SMTP y configurarlo
            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = true; // Habilita cifrado SSL
                client.Credentials = new NetworkCredential(username, password); // Credenciales de acceso

                // Construir remitente y destinatario
                var fromAddress = new MailAddress(fromEmail, displayName);
                var toAddress = new MailAddress(toEmail);

                // Crear el mensaje de correo
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Permite HTML en el cuerpo del correo
                })
                {
                    client.Send(message); // Enviar el mensaje
                }
            }
        }
    }
}
