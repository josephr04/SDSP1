using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

            var (exitoso, mensaje, idUsuario, nombre, correo, twoFactorEnabled, twoFactorSecret) =
                await _loginService.Login(model, ip);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View("Login", model);
            }

            TempData["UsuarioId"] = idUsuario;
            TempData["Correo"] = correo;
            TempData["Nombre"] = nombre;

            // ✅ Ya tiene 2FA configurado → ir a verificar código TOTP
            if (twoFactorEnabled && !string.IsNullOrEmpty(twoFactorSecret))
            {
                return RedirectToAction("Index", "TwoFactorSetup", new { usuarioId = idUsuario });
            }

            // ✅ No tiene 2FA → ir a configurarlo por primera vez
            return RedirectToAction("Index", "TwoFactorSetup", new { usuarioId = idUsuario });
        }
    }
}