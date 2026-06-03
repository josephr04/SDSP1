using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using SDSP1.Models;
using SDSP1.Services;
using SDSP1.Database;
using Dapper;

namespace SDSP1.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> Index(int? usuarioId)
        {
            try
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

                using var conn = _db.ObtenerConexion();
                await conn.OpenAsync();

                var usuario = await conn.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT two_factor_enabled, two_factor_secret FROM usuarios WHERE id_usuario = @id",
                    new { id = idUsuario }
                );

                if (usuario == null)
                    return RedirectToAction("Index", "Login");

                // Si el usuario no tiene 2FA habilitado, redirigir a configurarlo
                if (usuario.two_factor_enabled?.ToString() != "True" && usuario.two_factor_enabled?.ToString() != "1")
                {
                    return RedirectToAction("Index", "TwoFactorSetup", new { usuarioId = idUsuario });
                }

                TempData.Keep("UsuarioId");
                TempData.Keep("Correo");
                TempData.Keep("Nombre");

                var model = new Verify2FAViewModel { UsuarioId = idUsuario, Codigo = "" };

                ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador";

                return View("Autenticacion", model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(Verify2FAViewModel model)
        {
            try
            {
                // Validar código
                if (string.IsNullOrEmpty(model.Codigo) || model.Codigo.Length != 6 || !int.TryParse(model.Codigo, out _))
                {
                    ModelState.AddModelError("Codigo", "El código debe ser de 6 dígitos.");
                    ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador.";
                    return View("Autenticacion", model);
                }

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

                if (usuario.two_factor_enabled?.ToString() != "True" && usuario.two_factor_enabled?.ToString() != "1")
                {
                    ModelState.AddModelError("", "Error: 2FA no está habilitado.");
                    return RedirectToAction("Index", "TwoFactorSetup", new { usuarioId = model.UsuarioId });
                }

                // Descifrar secreto
                string secretoBase32 = _encryptionService.Decrypt((string)usuario.two_factor_secret);

                // Validar TOTP
                bool esValido = _totpService.ValidateCode(secretoBase32, model.Codigo);

                if (!esValido)
                {
                    ModelState.AddModelError("Codigo", "Código TOTP incorrecto. Verifica tu autenticador.");
                    ViewBag.Mensaje = "Ingresa el código de 6 dígitos que genera tu autenticador.";
                    return View("Autenticacion", model);
                }

                // ✅ Código válido - Crear sesión
                HttpContext.Session.SetString("autenticado", "true");
                HttpContext.Session.SetString("id_usuario", model.UsuarioId.ToString());

                var correo = TempData["Correo"]?.ToString() ?? "";
                var nombre = TempData["Nombre"]?.ToString() ?? "Usuario";

                HttpContext.Session.SetString("correo", correo);
                HttpContext.Session.SetString("nombre", nombre);

                // Limpiar TempData
                TempData.Remove("UsuarioId");
                TempData.Remove("Correo");
                TempData.Remove("Nombre");

                // Redirigir a Carpetas
                return RedirectToAction("Index", "Carpetas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Autenticacion", model);
            }
        }
    }
}