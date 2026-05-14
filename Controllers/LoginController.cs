using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Proyecto1_Seguridad.Database;

namespace Proyecto1_Seguridad.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            Conexion conexion = new Conexion();

            using (MySqlConnection conn =
                   conexion.ObtenerConexion())
            {
                conn.Open();
            }

            return View("Login");
        }
    }
}