using System.ComponentModel.DataAnnotations;

namespace SDSP1.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres")]
        public string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string contraseña { get; set; }
    }
}