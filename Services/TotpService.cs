using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace SDSP1.Services
{
    public class TotpService
    {
        private readonly EncryptionService _encryptionService;

        private const int CodeLength = 6;
        private const int TimeStep = 30;

        public TotpService(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public string GenerateSecret()
        {
            byte[] secretBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretBytes);
            }
            return ConvertToBase32(secretBytes);
        }

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
                result.Append(alphabet[(bitBuffer << (5 - bitCount)) & 31]);

            return result.ToString();
        }

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

        public string GenerateQrCodeUrl(string email, string appName, string secretBase32)
        {
            if (!IsValidBase32(secretBase32))
                throw new InvalidOperationException("Secreto TOTP inválido. Debe estar en Base32.");

            string url = $"otpauth://totp/{System.Web.HttpUtility.UrlEncode($"{appName}:{email}")}?" +
                        $"secret={secretBase32}&" +
                        $"issuer={System.Web.HttpUtility.UrlEncode(appName)}&" +
                        $"algorithm=SHA1&" +
                        $"digits={CodeLength}&" +
                        $"period={TimeStep}";

            return url;
        }

        public string GenerateQrCodeAsBase64(string otpauthUrl)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeImage = qrCode.GetGraphic(10);
                        return Convert.ToBase64String(qrCodeImage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al generar código QR", ex);
            }
        }

        public bool ValidateCode(string secretBase32, string inputCode)
        {
            if (string.IsNullOrEmpty(inputCode) || inputCode.Length != CodeLength || !int.TryParse(inputCode, out _))
                return false;

            try
            {
                byte[] secretBytes = ConvertFromBase32(secretBase32);
                long timeCounter = GetTimeCounter();

                // ✅ Ventana fija ±2 pasos = ±60 segundos de margen
                for (int i = -2; i <= 2; i++)
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

        private string GenerateTotpCode(byte[] secretKey, long timeCounter)
        {
            byte[] msg = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                msg[i] = (byte)(timeCounter & 0xff);
                timeCounter >>= 8;
            }

            using (var hmac = new HMACSHA1(secretKey))
            {
                byte[] hash = hmac.ComputeHash(msg);
                int offset = hash[hash.Length - 1] & 0x0f;

                int code = ((hash[offset] & 0x7f) << 24)
                         | ((hash[offset + 1] & 0xff) << 16)
                         | ((hash[offset + 2] & 0xff) << 8)
                         | (hash[offset + 3] & 0xff);

                code = code % (int)Math.Pow(10, CodeLength);
                return code.ToString().PadLeft(CodeLength, '0');
            }
        }

        private long GetTimeCounter()
        {
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTimestamp / TimeStep;
        }

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
                System.Diagnostics.Debug.WriteLine($"ERROR en GetCurrentCode: {ex.Message}");
                throw;
            }
        }

        private bool IsValidBase32(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[A-Z2-7]+=*$");
        }
    }
}