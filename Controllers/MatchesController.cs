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
        private readonly MatchesDAL _matchesDAL;
        private readonly PerfilDeUsuarioDAL _perfilDeUsuarioDAL; 
        public MatchesController(
    MatchesDAL matchesDAL,
    PerfilDeUsuarioDAL perfilDeUsuarioDAL)    
        {
            _matchesDAL = matchesDAL;
            _perfilDeUsuarioDAL = perfilDeUsuarioDAL; 
        }

        // GET: /Matches
        [HttpGet]
        public IActionResult Index()
        {
            // Leer ID de usuario de la cookie
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            
            var usuarios = _matchesDAL.GetMatchesForUser(userId);

            // Mapear user.Id → matchId para el chat
            var matchMap = new Dictionary<int, int>();
            foreach (var u in usuarios)
            {
                matchMap[u.Id] = _matchesDAL.GetConversationMatchId(userId, u.Id);
            }
            ViewBag.MatchMap = matchMap;

            //  Convertir a DashboardProfileViewModel para tener foto e intereses
            var model = usuarios.Select(u => {
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

        // POST: /Matches/Like
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Like(int targetId)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            if (!_matchesDAL.ExistsLike(userId, targetId))
            {
                _matchesDAL.InsertLike(userId, targetId);
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