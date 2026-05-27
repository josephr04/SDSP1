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
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("id_usuario"));
            var lista = await _carpetasService.ObtenerCarpetas(idUsuario);
            return View("Carpetas", lista);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCarpeta(string nombre)
        {
            int idUsuario = Convert.ToInt32(HttpContext.Session.GetString("id_usuario"));

            // Validar entrada
            var validacion = ValidarNombreCarpeta(nombre);
            if (!validacion.Valido)
            {
                TempData["ErrorMessage"] = validacion.Mensaje;
                return RedirectToAction("Index");
            }

            // Sanitizar el nombre
            string nombreSanitizado = SanitizarNombre(nombre);

            await _carpetasService.CrearCarpeta(idUsuario, nombreSanitizado);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Valida el nombre de la carpeta según reglas de negocio
        /// </summary>
        private (bool Valido, string Mensaje) ValidarNombreCarpeta(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre de la carpeta no puede estar vacío.");

            nombre = nombre.Trim();

            if (nombre.Length > NombreMaxLength)
                return (false, $"El nombre de la carpeta no puede exceder {NombreMaxLength} caracteres.");

            if (nombre.Length < 1)
                return (false, "El nombre de la carpeta debe tener al menos 1 carácter.");

            // Validar caracteres prohibidos
            var caracteresProhibidos = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            if (nombre.Any(c => caracteresProhibidos.Contains(c)))
                return (false, "El nombre contiene caracteres no permitidos: \\ / : * ? \" < > |");

            return (true, "");
        }

        /// <summary>
        /// Sanitiza el nombre de la carpeta para evitar ataques XSS
        /// </summary>
        private string SanitizarNombre(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return "";

            // Remover espacios en blanco al inicio y final
            nombre = nombre.Trim();

            // Remover caracteres de control y caracteres especiales peligrosos
            nombre = Regex.Replace(nombre, @"[\x00-\x1F\x7F]", "");

            return nombre;
        }
    }
}
