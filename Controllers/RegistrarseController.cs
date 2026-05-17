using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    public class RegistrarseController : Controller
    {
        private readonly RegistrarService _registrarService;

        public RegistrarseController(RegistrarService registrarService)
        {
            _registrarService = registrarService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Registrarse");
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(RegistrarViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Registrarse", model);

            var (exitoso, mensaje) = await _registrarService.Registrar(model);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View("Registrarse", model);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction("Index", "Login");
        }
    }
}