using Microsoft.AspNetCore.Mvc;

namespace SDSP1.Controllers
{
    public class SesionController : Controller
    {
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}