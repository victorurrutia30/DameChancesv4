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

        public MensajesController(MatchesDAL matchesDAL, MensajeDAL mensajeDAL)
        {
            _matchesDAL = matchesDAL;
            _mensajeDAL = mensajeDAL;
        }

        // GET: /Mensajes/Chat?matchId=5
        [HttpGet]
        public IActionResult Chat(int matchId)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // valida que el usuario forme parte del match
            var match = _matchesDAL.GetMatchById(matchId);
            if (match == null ||
                (match.Usuario1Id != userId && match.Usuario2Id != userId))
                return NotFound();

            // trae todos los mensajes
            var mensajes = _mensajeDAL.GetMensajesByMatch(matchId);
            // cuenta los no leídos para mostrar badge
            var noLeidos = _mensajeDAL.ContarNoLeidos(userId, matchId);

            ViewBag.Match = match;
            ViewBag.NoLeidos = noLeidos;
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

            // 1) Lista de usuarios con match mutuo
            var usuarios = _matchesDAL.GetMatchesForUser(userId);

            // 2) Para cada usuario, obtener el mismo conversationId y los no leídos
            var matchMap = new Dictionary<int, int>();
            var unreadMap = new Dictionary<int, int>();

            foreach (var u in usuarios)
            {
                int convoId = _matchesDAL.GetConversationMatchId(userId, u.Id);
                matchMap[u.Id] = convoId;
                unreadMap[u.Id] = _mensajeDAL.ContarNoLeidos(userId, convoId);
            }

            ViewBag.MatchMap = matchMap;
            ViewBag.UnreadMap = unreadMap;
            return View(usuarios);
        }
    }
}
