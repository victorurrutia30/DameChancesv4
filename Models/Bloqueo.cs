namespace DameChanceSV2.Models
{
    public class Bloqueo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int UsuarioBloqueadoId { get; set; }
        public DateTime FechaBloqueo { get; set; }
    }
}
