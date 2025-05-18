using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DameChanceSV2.Attributes
{
    // =============================================================================
    // MaxFileSizeAttribute
    // Atributo personalizado para validar el tamano máximo de un archivo subido.
    // USO: [MaxFileSize(2 * 1024 * 1024)] // 2 MB
    // =============================================================================
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxBytes;

        // Constructor: define el tamano máximo permitido en bytes
        public MaxFileSizeAttribute(int maxBytes) => _maxBytes = maxBytes;

        // Valida si el archivo supera el límite de tamano
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is IFormFile file && file.Length > _maxBytes)
                return new ValidationResult($"El tamano máximo permitido es {_maxBytes / (1024 * 1024)} MB.");

            return ValidationResult.Success;
        }
    }

    // =============================================================================
    // AllowedExtensionsAttribute
    // Atributo personalizado para validar extensiones permitidas de archivos subidos.
    // USO: [AllowedExtensions(new[] { ".jpg", ".png" })]
    // =============================================================================
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        // Constructor: recibe un arreglo con las extensiones válidas
        public AllowedExtensionsAttribute(string[] extensions) => _extensions = extensions;

        // Valida si la extensión del archivo está en la lista permitida
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!_extensions.Contains(ext))
                    return new ValidationResult($"Sólo se permiten: {string.Join(", ", _extensions)}");
            }

            return ValidationResult.Success;
        }
    }
}
