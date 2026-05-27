using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using SDSP1.Models;
using SDSP1.Services;
using SDSP1.Database;
using Dapper;

namespace SDSP1.Controllers
{
    /// <summary>
    /// Controlador para validar 2FA TOTP en el login
    /// El usuario debe haber escanado el código QR previamente en /TwoFactorSetup
    /// </summary>
    public class AutenticacionController : Controller
    {
        private readonly TotpService _totpService;
        private readonly EncryptionService _encryptionService;
        private readonly Conexion _db;

        public AutenticacionController(TotpService totpService, EncryptionService encryptionService, Conexion db)
        {
            _totpService = totpService;
            _encryptionService = encryptionService;
            _db = db;
        }

        /// <summary>
        /// GET: Mostrar pantalla de validación TOTP
        /// Se llama después de que LoginController verificó credenciales correctas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? usuarioId)
        {
            // Guardar datos en TempData si vienen en query string
            if (usuarioId.HasValue)
            {
                TempData["UsuarioId"] = usuarioId.Value;
            }

            // Validar que tenemos el ID del usuario
            if (TempData["UsuarioId"] == null)
                return RedirectToAction("Index", "Login");

            int idUsuario = (int)TempData["UsuarioId"];

            // Obtener usuario de BD para verificar si tiene 2FA habilitado
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT two_factor_enabled, two_factor_secret FROM usuarios WHERE id_usuario = @id",
                new { id = idUsuario }
            );

            if (usuario == null)
                return RedirectToAction("Index", "Login");

            // Si el usuario no tiene 2FA habilitado, redirigir a configurarlo
            if (usuario.two_factor_enabled == "0")
            {
                // Redirigir a configurar 2FA (es obligatorio)
                return RedirectToAction("Index", "TwoFactorSetup", 
                    new { usuarioId = idUsuario });
            }

            // Mantener datos en TempData para POST
            TempData.Keep("UsuarioId");
            TempData.Keep("Correo");
            TempData.Keep("Nombre");

            var model = new Verify2FAViewModel 
            { 
                UsuarioId = idUsuario,
                Codigo = ""
            };

            ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador (Google Authenticator, Microsoft Authenticator, Authy, etc.)";

            return View("Autenticacion", model);
        }

        /// <summary>
        /// POST: Validar código TOTP de 6 dígitos
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Index(Verify2FAViewModel model)
        {
            // Validar que el código sea de 6 dígitos
            if (string.IsNullOrEmpty(model.Codigo) || model.Codigo.Length != 6 || !int.TryParse(model.Codigo, out _))
            {
                ModelState.AddModelError("Codigo", "El código debe ser de 6 dígitos.");
                ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador.";
                return View("Autenticacion", model);
            }

            // Obtener el secreto cifrado de la BD
            using var conn = _db.ObtenerConexion();
            await conn.OpenAsync();

            var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT two_factor_secret, two_factor_enabled FROM usuarios WHERE id_usuario = @id",
                new { id = model.UsuarioId }
            );

            if (usuario == null || usuario.two_factor_secret == null)
            {
                ModelState.AddModelError("", "Error: No se encontró configuración de 2FA.");
                return View("Autenticacion", model);
            }

            // Verificar que 2FA está habilitado
            if (usuario.two_factor_enabled == "0")
            {
                ModelState.AddModelError("", "Error: 2FA no está habilitado para este usuario.");
                return View("Autenticacion", model);
            }

            // Descifrar el secreto
            string secretoBase32;
            try
            {
                secretoBase32 = _encryptionService.Decrypt((string)usuario.two_factor_secret);

                // DEBUG: Verificar que el secreto desificado es válido Base32
                if (string.IsNullOrEmpty(secretoBase32))
                {
                    throw new InvalidOperationException("Secreto descifrado es nulo o vacío");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al descifrar secreto: {ex.Message}");
                return View("Autenticacion", model);
            }

            // Validar el código TOTP
            if (!_totpService.ValidateCode(secretoBase32, model.Codigo))
            {
                ModelState.AddModelError("Codigo", "Código TOTP incorrecto. Verifica tu autenticador.");
                ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador.";
                return View("Autenticacion", model);
            }

            // ✅ Código TOTP válido - Establecer sesión y permitir acceso
            HttpContext.Session.SetString("autenticado", "true");
            HttpContext.Session.SetString("id_usuario", model.UsuarioId.ToString());

            // Recuperar datos del TempData (guardados en LoginController)
            var correo = TempData["Correo"]?.ToString() ?? "";
            var nombre = TempData["Nombre"]?.ToString() ?? "Usuario";

            HttpContext.Session.SetString("correo", correo);
            HttpContext.Session.SetString("nombre", nombre);

            // Actualizar fecha de última verificación exitosa en BD
            await conn.ExecuteAsync(
                "UPDATE usuarios SET two_factor_verified_at = NOW() WHERE id_usuario = @id",
                new { id = model.UsuarioId }
            );

            return RedirectToAction("Index", "Carpetas");
        }
    }
}