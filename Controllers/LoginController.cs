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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

            var (exitoso, mensaje) = await _loginService.Login(model, ip);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View(model);
            }

            HttpContext.Session.SetString("correo_temp", model.correo);

            // En el 2FA exitoso
            HttpContext.Session.SetString("correo", HttpContext.Session.GetString("correo_temp"));
            HttpContext.Session.SetString("autenticado", "true");
            HttpContext.Session.Remove("correo_temp"); // limpiar el temporal

            return RedirectToAction("Autenticacion", "Autenticacion");
        }
    }
}