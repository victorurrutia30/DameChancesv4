namespace DameChanceSV2.Models
{
    public class ConversationViewModel
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Genero { get; set; }
        public string Intereses { get; set; }
        public string Ubicacion { get; set; }
        public string ImagenPerfil { get; set; }
        public int MatchId { get; set; }
        public int UnreadCount { get; set; }
    }
}
