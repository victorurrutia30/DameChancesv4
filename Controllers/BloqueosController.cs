using DameChanceSV2.DAL;
using Microsoft.AspNetCore.Mvc;

namespace DameChanceSV2.Controllers
{
    public class BloqueosController : Controller
    {
        private readonly BloqueoDAL _bloqueoDAL;

        public BloqueosController(BloqueoDAL bloqueoDAL)
        {
            _bloqueoDAL = bloqueoDAL;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Bloquear(int idBloqueado)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            if (!_bloqueoDAL.ExisteBloqueo(userId, idBloqueado))
            {
                _bloqueoDAL.InsertBloqueo(userId, idBloqueado);
                TempData["BlockMsg"] = "Usuario bloqueado exitosamente.";
            }
            else
            {
                TempData["BlockMsg"] = "Ya habías bloqueado a este usuario.";
            }

            return RedirectToAction("Dashboard", "Home");
        }
    }
}
