using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SDSP1.Database;

namespace SDSP1.Controllers
{
    public class AutenticacionController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("Autenticacion");
        }

        [HttpPost]
        public IActionResult Autenticacion(string codigo)
        {
            // aquí va la lógica de verificar el código 2FA
            return View();
        }
    }
}