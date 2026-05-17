using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    public class RegistrarController : Controller
    {
        private readonly RegistrarService _registrarService;

        public RegistrarController(RegistrarService registrarService)
        {
            _registrarService = registrarService;
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(RegistrarViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (exitoso, mensaje) = await _registrarService.Registrar(model);

            if (!exitoso)
            {
                ModelState.AddModelError("", mensaje);
                return View(model);
            }

            TempData["Exito"] = mensaje;
            return RedirectToAction("Login", "Login");
        }
    }
}