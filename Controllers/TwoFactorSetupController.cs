using Microsoft.AspNetCore.Mvc;
using SDSP1.Database;
using SDSP1.Services;
using Dapper;

namespace SDSP1.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> Index(int? usuarioId = null)
        {
            try
            {
                // 1. Intentar obtener ID desde parámetro URL
                int? idUsuario = usuarioId;

                // 2. Si no, intentar desde TempData (viene del registro)
                if (idUsuario == null && TempData["UsuarioId"] != null)
                {
                    idUsuario = Convert.ToInt32(TempData["UsuarioId"]);
                    TempData.Keep("UsuarioId");
                    TempData.Keep("Correo");
                    TempData.Keep("Nombre");
                }

                // 3. Si no, intentar desde sesión
                if (idUsuario == null)
                {
                    var usuarioIdStr = HttpContext.Session.GetString("id_usuario");
                    if (!string.IsNullOrEmpty(usuarioIdStr) && int.TryParse(usuarioIdStr, out int sessionId))
                        idUsuario = sessionId;
                }

                // Si no hay ID, redirigir al login
                if (idUsuario == null)
                    return RedirectToAction("Index", "Login");

                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                // ✅ FIX: Consultar si el usuario ya tiene 2FA configurado
                var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT correo, nombre, two_factor_enabled, two_factor_secret FROM usuarios WHERE id_usuario = @id",
                    new { id = idUsuario }
                );

                if (usuario == null)
                    return RedirectToAction("Index", "Login");

                // ✅ FIX: Si ya tiene 2FA, redirigir a verificar — NO generar nuevo QR
                bool tieneSecret = !string.IsNullOrEmpty((string?)usuario.two_factor_secret);
                bool twoFaHabilitado = usuario.two_factor_enabled?.ToString() == "True" || usuario.two_factor_enabled?.ToString() == "1";

                if (twoFaHabilitado && tieneSecret)
                {
                    TempData["UsuarioId"] = idUsuario.Value;
                    TempData["Correo"] = (string)usuario.correo;
                    TempData["Nombre"] = (string)usuario.nombre;
                    return RedirectToAction("Index", "Autenticacion", new { usuarioId = idUsuario });
                }

                // A partir de aquí solo llegan usuarios SIN 2FA — primera configuración
                string email = TempData["Correo"] as string ?? (string)usuario.correo;
                string nombre = TempData["Nombre"] as string ?? (string)usuario.nombre;

                // Generar nuevo secreto TOTP
                string secretoBase32 = _totpService.GenerateSecret();

                // Guardar en TempData para validar en POST
                TempData[TempDataSecretKey] = secretoBase32;
                TempData["SetupUsuarioId"] = idUsuario.Value;

                // Generar URL otpauth:// para el código QR
                string otpauthUrl = _totpService.GenerateQrCodeUrl(email, "SDSP1", secretoBase32);

                // Generar imagen QR en Base64
                string qrCodeBase64 = _totpService.GenerateQrCodeAsBase64(otpauthUrl);

                ViewBag.QrCodeBase64 = qrCodeBase64;
                ViewBag.SecretoManual = secretoBase32;
                ViewBag.UsuarioEmail = email;

                if (TempData["ErrorTotp"] != null)
                    ViewBag.ErrorTotp = TempData["ErrorTotp"];

                return View("Setup2FA");
            }
            catch (Exception ex)
            {
                return Content($"Error al configurar 2FA: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string codigoTotp)
        {
            try
            {
                // Obtener ID del usuario
                int? usuarioId = null;

                // 1. Desde sesión
                var usuarioIdStr = HttpContext.Session.GetString("id_usuario");
                if (!string.IsNullOrEmpty(usuarioIdStr) && int.TryParse(usuarioIdStr, out int sessionId))
                    usuarioId = sessionId;

                // 2. Desde TempData
                if (usuarioId == null && TempData["SetupUsuarioId"] != null)
                {
                    usuarioId = Convert.ToInt32(TempData["SetupUsuarioId"]);
                    TempData.Keep("SetupUsuarioId");
                }

                if (usuarioId == null)
                    return RedirectToAction("Index", "Login");

                // Obtener secreto de TempData
                if (TempData[TempDataSecretKey] is not string secretoBase32)
                {
                    ModelState.AddModelError("", "Sesión expirada. Por favor intenta de nuevo.");
                    return RedirectToAction("Index", new { usuarioId = usuarioId });
                }

                // Validar el código
                if (string.IsNullOrEmpty(codigoTotp) || codigoTotp.Length != 6 || !int.TryParse(codigoTotp, out _))
                {
                    ModelState.AddModelError("codigoTotp", "El código debe ser de 6 dígitos.");
                    return RedirectToAction("Index", new { usuarioId = usuarioId });
                }

                if (!_totpService.ValidateCode(secretoBase32, codigoTotp))
                {
                    TempData["ErrorTotp"] = "Código incorrecto. Intenta con el código actual del autenticador.";
                    TempData[TempDataSecretKey] = secretoBase32;  // re-guardar para que no expire
                    TempData["SetupUsuarioId"] = usuarioId.Value; // re-guardar también
                    return RedirectToAction("Index", new { usuarioId = usuarioId });
                }

                // Guardar secreto cifrado en BD
                string secretoCifrado = _encryptionService.Encrypt(secretoBase32);
                string correo = "";
                string nombre = "";

                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT correo, nombre FROM usuarios WHERE id_usuario = @id",
                    new { id = usuarioId }
                );

                if (usuario != null)
                {
                    correo = usuario.correo;
                    nombre = usuario.nombre;
                }

                await conn.ExecuteAsync(
                    @"UPDATE usuarios 
                      SET two_factor_secret = @secret, 
                          two_factor_enabled = true, 
                          two_factor_verified_at = NOW()
                      WHERE id_usuario = @id",
                    new { secret = secretoCifrado, id = usuarioId }
                );

                // Limpiar TempData
                TempData.Remove(TempDataSecretKey);
                TempData.Remove("SetupUsuarioId");
                TempData.Remove("UsuarioId");
                TempData.Remove("Correo");
                TempData.Remove("Nombre");

                // Crear sesión
                HttpContext.Session.SetString("autenticado", "true");
                HttpContext.Session.SetString("id_usuario", usuarioId.ToString());
                HttpContext.Session.SetString("correo", correo);
                HttpContext.Session.SetString("nombre", nombre);

                return RedirectToAction("Index", "Carpetas");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View("Setup2FA");
            }
        }
    }
}