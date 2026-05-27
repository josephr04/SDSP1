using Microsoft.AspNetCore.Mvc;
using SDSP1.Database;
using SDSP1.Services;
using Dapper;

namespace SDSP1.Controllers
{
    /// <summary>
    /// Controlador para configurar autenticación de dos factores (TOTP)
    /// El usuario genera un secreto, escanea un código QR y verifica con su autenticador
    /// </summary>
    public class TwoFactorSetupController : Controller
    {
        private readonly TotpService _totpService;
        private readonly EncryptionService _encryptionService;
        private readonly Conexion _db;
        private const string TempDataSecretKey = "TotpSecret";

        public TwoFactorSetupController(TotpService totpService, EncryptionService encryptionService, Conexion db)
        {
            _totpService = totpService;
            _encryptionService = encryptionService;
            _db = db;
        }

        /// <summary>
        /// GET: Mostrar pantalla de configuración de 2FA con código QR
        /// Puede ser llamado durante login (sin sesión, con usuarioId en query) 
        /// o desde un panel settings (con sesión)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? usuarioId = null)
        {
            // Obtener ID del usuario desde sesión O desde query string
            int? idUsuario = usuarioId;

            if (idUsuario == null)
            {
                // Intentar obtener de sesión (si ya está logueado)
                var usuarioIdStr = HttpContext.Session.GetString("id_usuario");
                if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int sessionId))
                {
                    return RedirectToAction("Index", "Login");
                }
                idUsuario = sessionId;
            }

            // Obtener datos del usuario
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT correo, nombre FROM usuarios WHERE id_usuario = @id",
                new { id = idUsuario }
            );

            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Generar nuevo secreto TOTP
            string secretoBase32 = _totpService.GenerateSecret();

            // Guardar en TempData para validar en POST
            TempData[TempDataSecretKey] = secretoBase32;
            TempData["SetupUsuarioId"] = idUsuario.Value;

            // Generar URL otpauth:// para el código QR
            string otpauthUrl = _totpService.GenerateQrCodeUrl(
                (string)usuario.correo,
                "SDSP1",
                secretoBase32
            );

            // Generar imagen QR en Base64
            string qrCodeBase64 = _totpService.GenerateQrCodeAsBase64(otpauthUrl);

            ViewBag.QrCodeBase64 = qrCodeBase64;
            ViewBag.SecretoManual = secretoBase32;
            ViewBag.UsuarioEmail = usuario.correo;

            return View("Setup2FA");
        }

        /// <summary>
        /// POST: Verificar código TOTP y guardar secreto cifrado en BD
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Verify(string codigoTotp)
        {
            // Obtener ID del usuario desde sesión O TempData
            int? usuarioId = null;

            // Primero intentar sesión (si ya está logueado)
            var usuarioIdStr = HttpContext.Session.GetString("id_usuario");
            if (!string.IsNullOrEmpty(usuarioIdStr) && int.TryParse(usuarioIdStr, out int sessionId))
            {
                usuarioId = sessionId;
            }

            // Si no, obtener de TempData (durante login)
            if (usuarioId == null && TempData["SetupUsuarioId"] is int tempId)
            {
                usuarioId = tempId;
                TempData.Keep("SetupUsuarioId");
            }

            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Obtener secreto de TempData
            if (TempData[TempDataSecretKey] is not string secretoBase32)
            {
                ModelState.AddModelError("", "Sesión expirada. Por favor intenta de nuevo.");
                return RedirectToAction("Index", new { usuarioId = usuarioId });
            }

            // Validar que el código sea de 6 dígitos
            if (string.IsNullOrEmpty(codigoTotp) || codigoTotp.Length != 6 || !int.TryParse(codigoTotp, out _))
            {
                ModelState.AddModelError("codigoTotp", "El código debe ser de 6 dígitos.");

                // Regenerar QR para reintentar
                TempData[TempDataSecretKey] = secretoBase32;
                TempData["SetupUsuarioId"] = usuarioId;
                string otpauthUrl = _totpService.GenerateQrCodeUrl(
                    HttpContext.Session.GetString("correo") ?? "usuario@example.com",
                    "SDSP1",
                    secretoBase32
                );
                ViewBag.QrCodeBase64 = _totpService.GenerateQrCodeAsBase64(otpauthUrl);
                ViewBag.SecretoManual = secretoBase32;

                return View("Setup2FA");
            }

            // Validar el código TOTP
            if (!_totpService.ValidateCode(secretoBase32, codigoTotp))
            {
                ModelState.AddModelError("codigoTotp", "Código TOTP incorrecto. Verifica tu autenticador.");

                // Regenerar QR para reintentar
                TempData[TempDataSecretKey] = secretoBase32;
                TempData["SetupUsuarioId"] = usuarioId;
                string otpauthUrl = _totpService.GenerateQrCodeUrl(
                    HttpContext.Session.GetString("correo") ?? "usuario@example.com",
                    "SDSP1",
                    secretoBase32
                );
                ViewBag.QrCodeBase64 = _totpService.GenerateQrCodeAsBase64(otpauthUrl);
                ViewBag.SecretoManual = secretoBase32;

                return View("Setup2FA");
            }

            // ✅ Código válido - Guardar secreto cifrado en BD
            try
            {
                string secretoCifrado = _encryptionService.Encrypt(secretoBase32);

                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                // Obtener correo y nombre del usuario
                var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT correo, nombre FROM usuarios WHERE id_usuario = @id",
                    new { id = usuarioId }
                );


                await conn.ExecuteAsync(
                    @"UPDATE usuarios 
                      SET two_factor_secret = @secret, 
                          two_factor_enabled = '1', 
                          two_factor_verified_at = NOW()
                      WHERE id_usuario = @id",
                    new 
                    { 
                        secret = secretoCifrado,
                        id = usuarioId
                    }
                );

                // Limpiar TempData
                TempData.Remove(TempDataSecretKey);
                TempData.Remove("SetupUsuarioId");

                // Crear sesión automáticamente
                HttpContext.Session.SetString("autenticado", "true");
                HttpContext.Session.SetString("id_usuario", usuarioId.ToString());
                HttpContext.Session.SetString("correo", (string)usuario.correo);
                HttpContext.Session.SetString("nombre", (string)usuario.nombre);

                ViewBag.Exitoso = true;
                ViewBag.Mensaje = "¡2FA habilitado correctamente! Ahora deberás ingresar un código de 6 dígitos en cada login.";

                return View("Setup2FA");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar configuración: {ex.Message}");
                return View("Setup2FA");
            }
        }

        /// <summary>
        /// GET: Desactivar 2FA (opcional)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Disable()
        {
            var usuarioIdStr = HttpContext.Session.GetString("id_usuario");
            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int usuarioId))
            {
                return Unauthorized();
            }

            try
            {
                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                await conn.ExecuteAsync(
                    @"UPDATE usuarios 
                      SET two_factor_secret = NULL, 
                          two_factor_enabled = 0
                      WHERE id_usuario = @id",
                    new { id = usuarioId }
                );

                ViewBag.Mensaje = "2FA ha sido deshabilitado.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}
