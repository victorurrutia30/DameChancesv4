using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using DameChanceSV2.Attributes;

namespace DameChanceSV2.Models
{
    public class CompletarPerfilViewModel
    {
        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(18, 120, ErrorMessage = "La edad mínima es 18 años.")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El género es obligatorio.")]
        public string Genero { get; set; }

        [Display(Name = "Intereses (separados por coma)")]
        public string Intereses { get; set; }

        public string Ubicacion { get; set; }

        [Display(Name = "Imagen de perfil")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png" })]
        [MaxFileSize(2 * 1024 * 1024)]  // 2 MB
        public IFormFile ImagenPerfil { get; set; }
    }
}
