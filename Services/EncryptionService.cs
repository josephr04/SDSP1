using Microsoft.AspNetCore.DataProtection;

namespace SDSP1.Services
{
    /// <summary>
    /// Servicio para cifrar y descifrar datos sensibles usando DPAPI de ASP.NET Core
    /// Utiliza Entity Framework Data Protection para cifrado en reposo
    /// </summary>
    public class EncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider provider)
        {
            // Crear un protector específico para secretos 2FA
            _protector = provider.CreateProtector("SDSP1.Services.TwoFactorSecrets");
        }

        /// <summary>
        /// Cifra un string (ej. secreto TOTP) para almacenamiento seguro en BD
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            try
            {
                return _protector.Protect(plainText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al cifrar datos", ex);
            }
        }

        /// <summary>
        /// Descifra un string almacenado en BD para obtener el valor original
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;

            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al descifrar datos", ex);
            }
        }
    }
}
