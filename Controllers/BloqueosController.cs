using DameChanceSV2.DAL;
using DameChanceSV2.Models;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Desbloquear(int idBloqueado)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            _bloqueoDAL.EliminarBloqueo(userId, idBloqueado);
            TempData["BlockMsg"] = "Usuario desbloqueado correctamente.";

            return RedirectToAction("MisBloqueos");
        }


        [HttpGet]
        public IActionResult MisBloqueos()
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var bloqueadosIds = _bloqueoDAL.GetUsuariosBloqueados(userId);
            var usuarios = new List<Usuario>();

            foreach (var id in bloqueadosIds)
            {
                var u = _bloqueoDAL.GetUsuarioBloqueadoConInfo(id);
                if (u != null) usuarios.Add(u);
            }

            return View(usuarios);
        }


    }
}
