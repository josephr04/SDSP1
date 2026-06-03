using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;
using System.Text.RegularExpressions;

namespace SDSP1.Controllers
{
    [SesionActiva]
    public class CarpetasController : Controller
    {
        private readonly CarpetasService _carpetasService;
        private const int NombreMaxLength = 255;

        public CarpetasController(CarpetasService carpetasService)
        {
            _carpetasService = carpetasService;
        }

        public async Task<IActionResult> Index()
        {
            var lista = await _carpetasService.ObtenerCarpetas();
            return View("Carpetas", lista);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCarpeta(string nombre)
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("id_usuario"));

            var validacion = ValidarNombreCarpeta(nombre);
            if (!validacion.Valido)
            {
                TempData["ErrorMessage"] = validacion.Mensaje;
                return RedirectToAction("Index");
            }

            string nombreSanitizado = SanitizarNombre(nombre);

            try
            {
                await _carpetasService.CrearCarpeta(idUsuario, nombreSanitizado);
                TempData["SuccessMessage"] = "Carpeta creada exitosamente.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> EliminarCarpeta(int idCarpeta)
        {
            try
            {
                await _carpetasService.EliminarCarpeta(idCarpeta);
                TempData["SuccessMessage"] = "Carpeta eliminada.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        private (bool Valido, string Mensaje) ValidarNombreCarpeta(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre de la carpeta no puede estar vacío.");

            nombre = nombre.Trim();

            if (nombre.Length > NombreMaxLength)
                return (false, $"El nombre no puede exceder {NombreMaxLength} caracteres.");

            var caracteresProhibidos = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (nombre.Any(c => caracteresProhibidos.Contains(c)))
                return (false, "El nombre contiene caracteres no permitidos: \\ / : * ? \" < > |");

            return (true, "");
        }

        private string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "";

            nombre = nombre.Trim();
            nombre = Regex.Replace(nombre, @"[\x00-\x1F\x7F]", "");
            return nombre;
        }
    }
}