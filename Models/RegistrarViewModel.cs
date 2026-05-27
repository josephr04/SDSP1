using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SDSP1.Models
{
    public class RegistrarViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
            ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public required string nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres")]
        public required string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[!@#$%^&*()\[\]{}_+=<>?,.:`~|]).{8,}$",
            ErrorMessage = "La contraseña debe tener al menos una mayúscula y un carácter especial.")]
        public required string contraseña { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("contraseña", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public required string confirmarContraseña { get; set; }

        // Segunda capa: bloquea patrones SQL en todos los campos
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var patronesSql = new[]
            {
                "--", ";--", "/*", "*/", "xp_",
                "DROP", "SELECT", "INSERT", "DELETE",
                "UPDATE", "EXEC", "UNION", "OR 1=1"
            };

            var campos = new Dictionary<string, string>
            {
                { nameof(nombre),   nombre   ?? "" },
                { nameof(correo),   correo   ?? "" },
                { nameof(contraseña), contraseña ?? "" }
            };

            foreach (var campo in campos)
            {
                foreach (var patron in patronesSql)
                {
                    if (campo.Value.Contains(patron, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new ValidationResult(
                            "Se detectaron caracteres o palabras no permitidas en uno de los campos.",
                            new[] { campo.Key }
                        );
                        yield break;
                    }
                }
            }
        }
    }
}