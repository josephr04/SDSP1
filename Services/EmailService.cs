using MailKit.Net.Smtp;
using MimeKit;

namespace SDSP1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCodigoRecuperacion(string destinatario, string codigo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:DisplayName"],
                _config["Email:From"]
            ));
            message.To.Add(MailboxAddress.Parse(destinatario));
            message.Subject = "Código de recuperación - CloudDrive";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: Segoe UI, sans-serif; max-width: 500px; margin: auto; padding: 30px; border-radius: 15px; border: 1px solid #edf5f5;'>
                    <h2 style='color: #246b6b;'>Recuperación de contraseña</h2>
                    <p style='color: #577474;'>Tu código de recuperación es:</p>
                    <div style='background: #edf8f5; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0;'>
                        <span style='font-size: 2.5rem; font-weight: bold; color: #246b6b; letter-spacing: 10px;'>{codigo}</span>
                    </div>
                    <p style='color: #9bb0b0; font-size: 0.9rem;'>Este código expira en <strong>15 minutos</strong>.</p>
                    <p style='color: #9bb0b0; font-size: 0.9rem;'>Si no solicitaste esto, ignora este correo.</p>
                </div>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:From"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}