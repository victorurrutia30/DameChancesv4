using System.ComponentModel.DataAnnotations;


namespace DameChanceSV2.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "La confirmación de contrasena es obligatoria.")]
        [Compare("Contrasena", ErrorMessage = "Las contrasenas no coinciden.")]
        public string ConfirmarContrasena { get; set; }
    }
}
