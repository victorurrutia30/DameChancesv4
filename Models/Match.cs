namespace DameChanceSV2.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int Usuario1Id { get; set; }
        public int Usuario2Id { get; set; }
        public DateTime FechaMatch { get; set; }
    }
}
