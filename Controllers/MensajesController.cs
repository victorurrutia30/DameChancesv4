// ==========================================
// IMPORTACIONES Y DEPENDENCIAS
// ==========================================

using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using System.Linq;

namespace DameChanceSV2.Controllers
{
    public class MensajesController : Controller
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS
        // Acceso a datos de matches, mensajes, perfiles y usuarios
        // ==========================================
        private readonly MatchesDAL _matchesDAL;
        private readonly MensajeDAL _mensajeDAL;
        private readonly PerfilDeUsuarioDAL _perfilDAL;
        private readonly UsuarioDAL _usuarioDAL;

        public MensajesController(
            MatchesDAL matchesDAL,
            MensajeDAL mensajeDAL,
            PerfilDeUsuarioDAL perfilDAL,
            UsuarioDAL usuarioDAL)
        {
            _matchesDAL = matchesDAL;
            _mensajeDAL = mensajeDAL;
            _perfilDAL = perfilDAL;
            _usuarioDAL = usuarioDAL;
        }

        // ==========================================
        // VISTA DEL CHAT 1-A-1 ENTRE USUARIOS
        // GET: /Mensajes/Chat?matchId=xx
        // ==========================================
        [HttpGet]
        public IActionResult Chat(int matchId)
        {
            // Validar sesión
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Verificar si el match pertenece al usuario
            var match = _matchesDAL.GetMatchById(matchId);
            if (match == null ||
                (match.Usuario1Id != userId && match.Usuario2Id != userId))
                return NotFound();

            // Obtener número de mensajes no leídos
            int noLeidos = _mensajeDAL.ContarNoLeidos(userId, matchId);
            ViewBag.NoLeidos = noLeidos;

            // Obtener nombre del otro usuario
            int otherId = match.Usuario1Id == userId
                          ? match.Usuario2Id
                          : match.Usuario1Id;
            var other = _usuarioDAL.GetUsuarioById(otherId);
            ViewBag.OtherUserName = other?.Nombre ?? "Usuario";

            // Pasar datos a la vista
            ViewBag.Match = match;
            var mensajes = _mensajeDAL.GetMensajesByMatch(matchId);
            return View(mensajes);
        }

        // ==========================================
        // ENVÍO DE UN NUEVO MENSAJE EN UN MATCH
        // POST: /Mensajes/Enviar
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enviar(int matchId, string contenido)
        {
            // Validar sesión
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Obtener receptor a partir del match
            var match = _matchesDAL.GetMatchById(matchId);
            int receptor = match.Usuario1Id == userId ? match.Usuario2Id : match.Usuario1Id;

            // Crear y guardar mensaje
            var msg = new Mensaje
            {
                MatchId = matchId,
                EmisorId = userId,
                ReceptorId = receptor,
                Contenido = contenido
            };
            _mensajeDAL.InsertMensaje(msg);

            // Redirigir al mismo chat
            return RedirectToAction("Chat", new { matchId });
        }

        // ==========================================
        // LISTADO DE CONVERSACIONES ACTIVAS
        // GET: /Mensajes
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            // Validar sesión
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Obtener todos los matches del usuario
            var usuarios = _matchesDAL.GetMatchesForUser(userId);

            // Armar modelo de conversaciones con cantidad de mensajes no leídos
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
