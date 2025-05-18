using System.ComponentModel.DataAnnotations;

namespace DameChanceSV2.Models
{
    public class Reporte
    {
        public int Id { get; set; }

        // Quien reporta (se asigna en controller)
        public int IdReportante { get; set; }

        // Usuario reportado (se rellena en el GET del form)
        public int IdReportado { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(1000, ErrorMessage = "El motivo no puede exceder 1000 caracteres.")]
        public string Motivo { get; set; }

        public DateTime FechaReporte { get; set; }

        // Marcar si el admin ya resolvió el caso
        public bool Resuelto { get; set; }
        public string? MensajeResolucion { get; set; }
    }
}
