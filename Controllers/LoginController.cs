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

            var (exitoso, mensaje, idUsuario, nombre) = await _loginService.Login(model, ip);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View("Login", model);
            }

            // En el 2FA exitoso
            HttpContext.Session.SetString("autenticado", "true");

            HttpContext.Session.SetString("correo", model.correo);
            HttpContext.Session.SetString("id_usuario", idUsuario.ToString());
            HttpContext.Session.SetString("nombre", nombre);


            return RedirectToAction("Index", "Autenticacion");
        }
    }
}