using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using System;
using Microsoft.AspNetCore.Http;
using Azure.Core;
using DameChanceSV2.Models;

namespace DameChanceSV2.Controllers
{
    public class MatchesController : Controller
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS
        // ==========================================
        private readonly MatchesDAL _matchesDAL;
        private readonly PerfilDeUsuarioDAL _perfilDeUsuarioDAL;

        public MatchesController(
            MatchesDAL matchesDAL,
            PerfilDeUsuarioDAL perfilDeUsuarioDAL)
        {
            _matchesDAL = matchesDAL;
            _perfilDeUsuarioDAL = perfilDeUsuarioDAL;
        }

        // ==========================================
        // LISTADO DE MATCHES DEL USUARIO ACTUAL
        // GET: /Matches
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            // Validar si hay sesión activa (usuario logueado)
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Obtener usuarios con los que ha hecho match
            var usuarios = _matchesDAL.GetMatchesForUser(userId);

            // Crear diccionario userId → matchId para enlazar con el chat
            var matchMap = new Dictionary<int, int>();
            foreach (var u in usuarios)
            {
                matchMap[u.Id] = _matchesDAL.GetConversationMatchId(userId, u.Id);
            }
            ViewBag.MatchMap = matchMap;

            // Construir modelo con datos de perfil para cada match
            var model = usuarios.Select(u =>
            {
                var perfil = _perfilDeUsuarioDAL.GetPerfilByUsuarioId(u.Id);
                return new DashboardProfileViewModel
                {
                    UsuarioId = u.Id,
                    Nombre = u.Nombre,
                    Edad = perfil?.Edad ?? 0,
                    Genero = perfil?.Genero ?? "",
                    Intereses = perfil?.Intereses ?? "",
                    Ubicacion = perfil?.Ubicacion ?? "",
                    ImagenPerfil = perfil?.ImagenPerfil
                };
            }).ToList();

            return View(model);
        }

        // ==========================================
        // ACCIÓN PARA DAR "LIKE" A OTRO USUARIO
        // POST: /Matches/Like
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Like(int targetId)
        {
            // Validar si hay sesión activa (usuario logueado)
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Si no ha dado like antes, registrar el like
            if (!_matchesDAL.ExistsLike(userId, targetId))
            {
                _matchesDAL.InsertLike(userId, targetId);

                // Verificar si el like es recíproco (match)
                if (_matchesDAL.IsReciprocal(userId, targetId))
                    TempData["MatchMsg"] = "¡Tienes un nuevo match!";
                else
                    TempData["MatchMsg"] = "Has marcado “Me interesa” correctamente.";
            }
            else
            {
                TempData["MatchMsg"] = "Ya habías marcado “Me interesa” a esta persona.";
            }

            return RedirectToAction("Dashboard", "Home");
        }
    }
}
