using Microsoft.AspNetCore.Mvc;
using SDSP1.Models;
using SDSP1.Services;

namespace SDSP1.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecuperacionService _recuperacionService;

        public AccountController(RecuperacionService recuperacionService)
        {
            _recuperacionService = recuperacionService;
        }

        // GET: Se ejecuta cuando el usuario hace clic en "Olvidé la contraseña"
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Se ejecuta cuando el usuario envía el formulario con su correo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Generar código de recuperación
            var (exitoso, mensaje, codigo) = await _recuperacionService.GenerarCodigoRecuperacion(model.correo);

            // Guardar correo en sesión para uso posterior
            HttpContext.Session.SetString("RecuperacionCorreo", model.correo);

            // Seguridad: Siempre redirigir a confirmación, sin revelar si el correo existe
            // En producción, aquí enviarías el código por email
            if (exitoso)
            {
                // TODO: Enviar código por email
                System.Diagnostics.Debug.WriteLine($"Código de recuperación: {codigo}");
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // GET: Página de confirmación después de enviar correo
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: Formulario para ingresar código y nueva contraseña
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: Procesar cambio de contraseña
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validar que las contraseñas coincidan
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Las contraseñas no coinciden.");
                return View(model);
            }

            // Cambiar contraseña
            var (exitoso, mensaje) = await _recuperacionService.CambiarContraseña(model.Codigo, model.Password);

            if (!exitoso)
            {
                ModelState.AddModelError(string.Empty, mensaje);
                return View(model);
            }

            // Limpiar sesión
            HttpContext.Session.Remove("RecuperacionCorreo");

            return RedirectToAction("ResetPasswordConfirmation");
        }

        // GET: Página de confirmación después de restablecer contraseña
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
