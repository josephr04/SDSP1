using Microsoft.AspNetCore.Mvc;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    /// <summary>
    /// Controlador de DEBUG solo para desarrollo
    /// Muestra el código TOTP actual para testing
    /// REMOVER EN PRODUCCIÓN
    /// </summary>
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly TotpService _totpService;

        public DebugController(TotpService totpService)
        {
            _totpService = totpService;
        }

        /// <summary>
        /// DEBUG: Genera el código TOTP actual para un secreto Base32
        /// Uso: GET /api/debug/totp-code?secret=JBSWY3DPEBLW64TMMQ======
        /// </summary>
        [HttpGet("totp-code")]
        public IActionResult GetCurrentTotpCode(string secret)
        {
            if (string.IsNullOrEmpty(secret))
                return BadRequest("Parámetro 'secret' requerido");

            try
            {
                string code = _totpService.GetCurrentCode(secret);
                if (code == null)
                    return BadRequest("No se pudo generar código TOTP");

                return Ok(new
                {
                    success = true,
                    message = "Código TOTP actual",
                    secret = secret,
                    currentCode = code,
                    expiresIn = "30 segundos",
                    note = "El código cambia cada 30 segundos. Cópialo e ingresa inmediatamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DEBUG: Genera un secreto TOTP nuevo para testing
        /// </summary>
        [HttpGet("generate-secret")]
        public IActionResult GenerateSecret()
        {
            try
            {
                string secret = _totpService.GenerateSecret();

                string code;
                try
                {
                    code = _totpService.GetCurrentCode(secret);
                }
                catch (Exception ex)
                {
                    return Ok(new
                    {
                        success = true,
                        secret = secret,
                        currentCode = (string)null,
                        error = $"Error al generar código: {ex.Message}",
                        stackTrace = ex.StackTrace
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Nuevo secreto TOTP generado",
                    secret = secret,
                    currentCode = code,
                    note = "Usa este secreto para testing. El código cambia cada 30 segundos."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
