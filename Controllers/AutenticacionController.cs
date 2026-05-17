using Microsoft.AspNetCore.Mvc;

namespace SDSP1.Controllers
{
    public class AutenticacionController : Controller
    {
        [HttpGet]
        public IActionResult Autenticacion()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Autenticacion(string codigo)
        {
            // aquí va la lógica de verificar el código 2FA
            return View();
        }
    }
}