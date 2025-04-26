namespace DameChanceSV2.Models
{
    public class Mensaje
    {
        public int Id { get; set; }
        public int MatchId { get; set; }    
        public int EmisorId { get; set; }
        public int ReceptorId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Leido { get; set; }
    }
}
