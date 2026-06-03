using System.ComponentModel.DataAnnotations;

namespace SDSP1.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public required string correo { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos.")]
        public required string Codigo { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}
