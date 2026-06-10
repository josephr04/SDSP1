using System.Text;
using System.Text.Json;

namespace SDSP1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public EmailService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task EnviarCodigoRecuperacion(string destinatario, string codigo)
        {
            var payload = new
            {
                from = $"{_config["Email:DisplayName"]} <noreply@josephrosas.dev>",
                to = new[] { destinatario },
                subject = "Código de recuperación - CloudDrive",
                html = $@"
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

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["Email:ResendApiKey"]}");

            var response = await _http.PostAsync(
                "https://api.resend.com/emails",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al enviar correo: {error}");
            }
        }
    }
}