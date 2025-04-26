using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using System;
using Microsoft.AspNetCore.Http;
using Azure.Core;

namespace DameChanceSV2.Controllers
{
    public class MatchesController : Controller
    {
        private readonly MatchesDAL _matchesDAL;
        public MatchesController(MatchesDAL matchesDAL) => _matchesDAL = matchesDAL;

        // GET: /Matches
        [HttpGet]
        public IActionResult Index()
        {
            // Leer ID de usuario de la cookie
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var matches = _matchesDAL.GetMatchesForUser(userId);
            return View(matches);
        }

        // POST: /Matches/Like
        [HttpPost]
        public IActionResult Like(int targetId)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Registrar el like
            _matchesDAL.InsertLike(userId, targetId);

            // Si ya existía el inverso, es un match
            if (_matchesDAL.IsReciprocal(userId, targetId))
            {
                TempData["MatchMsg"] = "¡Tienes un nuevo match!";
            }

            return RedirectToAction("Dashboard", "Home");
        }
    }
}
