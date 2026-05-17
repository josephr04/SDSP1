using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;
using System.Diagnostics;

namespace SDSP1.Controllers
{
    [SesionActiva]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
