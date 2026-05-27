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
                return View(model);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

            var (exitoso, mensaje, idUsuario, nombre, celular) = await _loginService.Login(model, ip);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View("Login", model);
            }

            // Pasar datos al 2FA sin establecer sesión aún
            // Guardamos los datos en TempData para recuperarlos después del 2FA
            TempData["UsuarioId"] = idUsuario;
            TempData["Celular"] = celular;
            TempData["Correo"] = model.correo;
            TempData["Nombre"] = nombre;

            return RedirectToAction("Index", "Autenticacion", new 
            { 
                usuarioId = idUsuario,
                celular = celular 
            });
        }
    }
}