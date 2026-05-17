using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    [SesionActiva]
    public class CarpetasController : Controller
    {
        private readonly CarpetasService _carpetasService;

        public CarpetasController(CarpetasService carpetasService)
        {
            _carpetasService = carpetasService;
        }

        public async Task<IActionResult> Index()
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("id_usuario"));
            var lista = await _carpetasService.ObtenerCarpetas(idUsuario);
            return View("Carpetas", lista);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCarpeta(string nombre)
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("id_usuario"));
            await _carpetasService.CrearCarpeta(idUsuario, nombre);
            return RedirectToAction("Index");
        }
    }
}