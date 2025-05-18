using System.ComponentModel.DataAnnotations;

namespace DameChanceSV2.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        public string Contrasena { get; set; }
    }
}
