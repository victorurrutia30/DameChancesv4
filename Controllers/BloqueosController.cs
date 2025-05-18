using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using Microsoft.AspNetCore.Mvc;

namespace DameChanceSV2.Controllers
{
    public class BloqueosController : Controller
    {
        // ========================================
        // INYECCIÓN DE DEPENDENCIA AL DAL DE BLOQUEO
        // ========================================
        private readonly BloqueoDAL _bloqueoDAL;

        public BloqueosController(BloqueoDAL bloqueoDAL)
        {
            _bloqueoDAL = bloqueoDAL;
        }

        // ========================================
        // ACCIÓN PARA BLOQUEAR A UN USUARIO
        // POST: /Bloqueos/Bloquear
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Bloquear(int idBloqueado)
        {
            // Validar si hay sesión activa (user logueado)
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Verificar si ya existe el bloqueo
            if (!_bloqueoDAL.ExisteBloqueo(userId, idBloqueado))
            {
                _bloqueoDAL.InsertBloqueo(userId, idBloqueado);
                TempData["BlockMsg"] = "Usuario bloqueado exitosamente.";
            }
            else
            {
                TempData["BlockMsg"] = "Ya habías bloqueado a este usuario.";
            }

            // Redirige al dashboard luego del bloqueo
            return RedirectToAction("Dashboard", "Home");
        }

        // ========================================
        // ACCIÓN PARA DESBLOQUEAR A UN USUARIO
        // POST: /Bloqueos/Desbloquear
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Desbloquear(int idBloqueado)
        {
            // Validar si hay sesión activa (user logueado)
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Elimina el bloqueo y muestra mensaje
            _bloqueoDAL.EliminarBloqueo(userId, idBloqueado);
            TempData["BlockMsg"] = "Usuario desbloqueado correctamente.";

            return RedirectToAction("MisBloqueos");
        }

        // ========================================
        // LISTADO DE TODOS LOS USUARIOS BLOQUEADOS POR EL USUARIO ACTUAL
        // GET: /Bloqueos/MisBloqueos
        // ========================================
        [HttpGet]
        public IActionResult MisBloqueos()
        {
            // Validar si hay sesión activa (user logueado)
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Obtener IDs bloqueados y luego sus datos completos
            var bloqueadosIds = _bloqueoDAL.GetUsuariosBloqueados(userId);
            var usuarios = new List<Usuario>();

            foreach (var id in bloqueadosIds)
            {
                var u = _bloqueoDAL.GetUsuarioBloqueadoConInfo(id);
                if (u != null) usuarios.Add(u);
            }

            // Retorna la vista con los usuarios bloqueados
            return View(usuarios);
        }
    }
}
