using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace SDSP1.Services
{
    /// <summary>
    /// Servicio para gestionar autenticación TOTP (Time-based One-Time Password)
    /// Genera y valida códigos de 6 dígitos que cambian cada 30 segundos
    /// Implementación manual sin dependencias externas complicadas
    /// </summary>
    public class TotpService
    {
        private readonly EncryptionService _encryptionService;

        // Parámetros TOTP estándar RFC 6238
        private const int CodeLength = 6; // 6 dígitos
        private const int TimeStep = 30; // cada 30 segundos
        private const int TimeWindowMinutes = 1; // Aceptar códigos dentro de ±1 minuto

        public TotpService(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Genera un secreto TOTP aleatorio nuevo en formato Base32
        /// Este secreto se almacenará cifrado en la BD
        /// </summary>
        public string GenerateSecret()
        {
            // Generar 20 bytes aleatorios (160 bits) - estándar RFC 4226
            byte[] secretBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }

            // Convertir a Base32 manualmente
            return ConvertToBase32(secretBytes);
        }

        /// <summary>
        /// Convierte bytes a string Base32
        /// </summary>
        private string ConvertToBase32(byte[] input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            int bitBuffer = 0;
            int bitCount = 0;

            foreach (byte b in input)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitCount += 8;

                while (bitCount >= 5)
                {
                    bitCount -= 5;
                    result.Append(alphabet[(bitBuffer >> bitCount) & 31]);
                }
            }

            if (bitCount > 0)
            {
                result.Append(alphabet[(bitBuffer << (5 - bitCount)) & 31]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Convierte string Base32 a bytes
        /// </summary>
        private byte[] ConvertFromBase32(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new List<byte>();
            int bitBuffer = 0;
            int bitCount = 0;

            foreach (char c in input.ToUpper())
            {
                if (!alphabet.Contains(c))
                    throw new InvalidOperationException($"Carácter inválido en Base32: {c}");

                int value = alphabet.IndexOf(c);
                bitBuffer = (bitBuffer << 5) | value;
                bitCount += 5;

                if (bitCount >= 8)
                {
                    bitCount -= 8;
                    result.Add((byte)((bitBuffer >> bitCount) & 255));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Genera la URL otpauth:// para renderizar como código QR
        /// Parámetros: usuario@email.com y secreto Base32
        /// </summary>
        public string GenerateQrCodeUrl(string email, string appName, string secretBase32)
        {
            // Validar que el secreto está en Base32
            if (!IsValidBase32(secretBase32))
                throw new InvalidOperationException("Secreto TOTP inválido. Debe estar en Base32.");

            // Formato estándar: otpauth://totp/Label?secret=SECRET&issuer=ISSUER
            string url = $"otpauth://totp/{System.Web.HttpUtility.UrlEncode($"{appName}:{email}")}?" +
                        $"secret={secretBase32}&" +
                        $"issuer={System.Web.HttpUtility.UrlEncode(appName)}&" +
                        $"algorithm=SHA1&" +
                        $"digits={CodeLength}&" +
                        $"period={TimeStep}";

            return url;
        }

        /// <summary>
        /// Genera un código QR como imagen PNG desde la URL otpauth://
        /// Retorna el código QR en formato Base64 para incrustar en HTML
        /// </summary>
        public string GenerateQrCodeAsBase64(string otpauthUrl)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(10); // 10px por módulo
                        return Convert.ToBase64String(qrCodeImage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al generar código QR", ex);
            }
        }

        /// <summary>
        /// Valida que un código TOTP de 6 dígitos sea correcto
        /// Acepta el código actual y del minuto anterior/siguiente (ventana de ±1 minuto)
        /// </summary>
        public bool ValidateCode(string secretBase32, string inputCode)
        {
            if (string.IsNullOrEmpty(inputCode) || inputCode.Length != CodeLength || !int.TryParse(inputCode, out _))
                return false;

            try
            {
                // Decodificar secreto de Base32 a bytes
                byte[] secretBytes = ConvertFromBase32(secretBase32);

                // Obtener tiempo actual en pasos de 30 segundos
                long timeCounter = GetTimeCounter();

                // Validar con ventana de tiempo (±1 minuto = ±2 pasos de 30seg)
                int windowSize = (int)(TimeWindowMinutes / TimeStep);

                for (int i = -windowSize; i <= windowSize; i++)
                {
                    string code = GenerateTotpCode(secretBytes, timeCounter + i);
                    if (code == inputCode)
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Genera el código TOTP para un tiempo específico
        /// </summary>
        private string GenerateTotpCode(byte[] secretKey, long timeCounter)
        {
            // Crear mensaje con el contador de tiempo en formato big-endian
            byte[] msg = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                msg[i] = (byte)(timeCounter & 0xff);
                timeCounter >>= 8;
            }

            // Calcular HMAC-SHA1
            using (var hmac = new HMACSHA1(secretKey))
            {
                byte[] hash = hmac.ComputeHash(msg);

                // Obtener offset dinámico del último byte
                int offset = hash[hash.Length - 1] & 0x0f;

                // Extraer 4 bytes desde el offset
                int code = ((hash[offset] & 0x7f) << 24)
                         | ((hash[offset + 1] & 0xff) << 16)
                         | ((hash[offset + 2] & 0xff) << 8)
                         | (hash[offset + 3] & 0xff);

                // Obtener últimos 6 dígitos
                code = code % (int)Math.Pow(10, CodeLength);

                return code.ToString().PadLeft(CodeLength, '0');
            }
        }

        /// <summary>
        /// Obtiene el contador de tiempo actual en pasos de 30 segundos
        /// </summary>
        private long GetTimeCounter()
        {
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTimestamp / TimeStep;
        }

        /// <summary>
        /// Obtiene el código TOTP actual (para debug/testing SOLAMENTE)
        /// NO usar en producción - es solo referencia
        /// </summary>
        public string GetCurrentCode(string secretBase32)
        {
            try
            {
                byte[] secretBytes = ConvertFromBase32(secretBase32);
                long timeCounter = GetTimeCounter();
                return GenerateTotpCode(secretBytes, timeCounter);
            }
            catch (Exception ex)
            {
                // DEBUG: Log the actual exception
                System.Diagnostics.Debug.WriteLine($"ERROR en GetCurrentCode: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                throw; // Re-lanzar para ver el error en debug
            }
        }

        /// <summary>
        /// Valida que un string sea un secreto válido en Base32
        /// </summary>
        private bool IsValidBase32(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Base32 usa solo caracteres A-Z y 2-7
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[A-Z2-7]+=*$");
        }
    }
}
