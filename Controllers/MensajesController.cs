using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using System.Linq;

namespace DameChanceSV2.Controllers 
{
    public class MensajesController : Controller
    {
        private readonly MatchesDAL _matchesDAL;
        private readonly MensajeDAL _mensajeDAL;
        private readonly PerfilDeUsuarioDAL _perfilDAL;  // <-- nuevo
        private readonly UsuarioDAL _usuarioDAL;         // <-- nuevo

        public MensajesController(
            MatchesDAL matchesDAL,
            MensajeDAL mensajeDAL,
            PerfilDeUsuarioDAL perfilDAL,
            UsuarioDAL usuarioDAL)                 // <-- nuevo
        {
            _matchesDAL = matchesDAL;
            _mensajeDAL = mensajeDAL;
            _perfilDAL = perfilDAL;              // <-- inicialización
            _usuarioDAL = usuarioDAL;             // <-- inicialización
        }

        // GET: /Mensajes/Chat?matchId=5
        [HttpGet]
        public IActionResult Chat(int matchId)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var match = _matchesDAL.GetMatchById(matchId);
            if (match == null ||
                (match.Usuario1Id != userId && match.Usuario2Id != userId))
                return NotFound();

            // No leídos
            int noLeidos = _mensajeDAL.ContarNoLeidos(userId, matchId);
            ViewBag.NoLeidos = noLeidos;

            // Nombre del otro usuario
            int otherId = match.Usuario1Id == userId
                          ? match.Usuario2Id
                          : match.Usuario1Id;
            var other = _usuarioDAL.GetUsuarioById(otherId);
            ViewBag.OtherUserName = other?.Nombre ?? "Usuario";

            ViewBag.Match = match;
            var mensajes = _mensajeDAL.GetMensajesByMatch(matchId);
            return View(mensajes);
        }

        // POST: /Mensajes/Enviar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enviar(int matchId, string contenido)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var match = _matchesDAL.GetMatchById(matchId);
            int receptor = match.Usuario1Id == userId ? match.Usuario2Id : match.Usuario1Id;

            var msg = new Mensaje
            {
                MatchId = matchId,
                EmisorId = userId,
                ReceptorId = receptor,
                Contenido = contenido
            };
            _mensajeDAL.InsertMensaje(msg);
            return RedirectToAction("Chat", new { matchId });
        }

        // GET: /Mensajes
        [HttpGet]
        public IActionResult Index()
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // 1) Todos los matches mutuos
            var usuarios = _matchesDAL.GetMatchesForUser(userId);

            // 2) Construir conversación + conteo no leídos
            var list = new List<ConversationViewModel>();
            foreach (var u in usuarios)
            {
                int convoId = _matchesDAL.GetConversationMatchId(userId, u.Id);
                int unread = _mensajeDAL.ContarNoLeidos(userId, convoId);
                var perfil = _perfilDAL.GetPerfilByUsuarioId(u.Id);
                list.Add(new ConversationViewModel
                {
                    UsuarioId = u.Id,
                    Nombre = u.Nombre,
                    Edad = perfil?.Edad ?? 0,
                    Genero = perfil?.Genero ?? "",
                    Intereses = perfil?.Intereses ?? "",
                    Ubicacion = perfil?.Ubicacion ?? "",
                    ImagenPerfil = perfil?.ImagenPerfil,
                    MatchId = convoId,
                    UnreadCount = unread
                });
            }

            return View(list);
        }
    }
}
