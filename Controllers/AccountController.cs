using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecuperacionService _recuperacionService;
        private readonly EmailService _emailService;

        public AccountController(RecuperacionService recuperacionService, EmailService emailService)
        {
            _recuperacionService = recuperacionService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (exitoso, mensaje, codigo) = await _recuperacionService.GenerarCodigoRecuperacion(model.correo);

            HttpContext.Session.SetString("RecuperacionCorreo", model.correo);

            if (exitoso)
            {
                try
                {
                    await _emailService.EnviarCodigoRecuperacion(model.correo, codigo);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Error al enviar el correo. Intenta de nuevo.");
                    return View(model);
                }
            }

            // Siempre redirigir para no revelar si el correo existe
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                return View(model);
            }

            var (exitoso, mensaje) = await _recuperacionService.CambiarContraseña(model.Codigo, model.Password);

            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                return View(model);
            }

            HttpContext.Session.Remove("RecuperacionCorreo");

            return RedirectToAction("ResetPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}