using System.ComponentModel.DataAnnotations;

namespace SDSP1.Models
{
    public class RegistrarViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string contraseña { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("contraseña", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string confirmarContraseña { get; set; }
    }
}