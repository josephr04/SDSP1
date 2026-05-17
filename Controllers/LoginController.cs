using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SDSP1.Database;

namespace SDSP1.Controllers
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