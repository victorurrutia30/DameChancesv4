using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using Microsoft.AspNetCore.Mvc;

namespace DameChanceSV2.Controllers
{
    public class RolesController : Controller
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS
        // ==========================================
        private readonly RolDAL _rolDAL;
        private readonly UsuarioDAL _usuarioDAL;

        public RolesController(RolDAL rolDAL, UsuarioDAL usuarioDAL)
        {
            _rolDAL = rolDAL;
            _usuarioDAL = usuarioDAL;
        }

        // ==========================================
        // FUNCIÓN PRIVADA PARA VALIDAR SI ES ADMIN
        // ==========================================
        private bool EsAdmin()
        {
            var cookie = Request.Cookies["UserSession"];
            if (!int.TryParse(cookie, out int userId)) return false;

            var user = _usuarioDAL.GetUsuarioById(userId);
            return user?.RolId == 1;
        }

        // ==========================================
        // LISTADO DE ROLES CON ESTADO DE USO
        // GET: /Roles
        // ==========================================
        public IActionResult Index()
        {
            if (!EsAdmin()) return NotFound();

            var roles = _rolDAL.GetRoles();
            var rolesEnUso = new Dictionary<int, bool>();

            // Determinar si cada rol está en uso por algún usuario
            foreach (var rol in roles)
            {
                rolesEnUso[rol.Id] = _rolDAL.EstaRolEnUso(rol.Id);
            }

            ViewBag.RolesEnUso = rolesEnUso;

            return View(roles);
        }

        // ==========================================
        // FORMULARIO PARA CREAR NUEVO ROL
        // GET: /Roles/Create
        // ==========================================
        public IActionResult Create()
        {
            if (!EsAdmin()) return NotFound();
            return View();
        }

        // ==========================================
        // GUARDAR NUEVO ROL
        // POST: /Roles/Create
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Rol rol)
        {
            if (!EsAdmin()) return NotFound();

            if (ModelState.IsValid)
            {
                _rolDAL.InsertRol(rol);
                return RedirectToAction("Index");
            }

            return View(rol);
        }

        // ==========================================
        // FORMULARIO PARA EDITAR ROL EXISTENTE
        // GET: /Roles/Edit/{id}
        // ==========================================
        public IActionResult Edit(int id)
        {
            if (!EsAdmin()) return NotFound();

            var rol = _rolDAL.GetRolById(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        // ==========================================
        // GUARDAR EDICIÓN DE ROL
        // POST: /Roles/Edit
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Rol rol)
        {
            if (!EsAdmin()) return NotFound();

            if (ModelState.IsValid)
            {
                _rolDAL.UpdateRol(rol);
                return RedirectToAction("Index");
            }

            return View(rol);
        }

        // ==========================================
        // CONFIRMAR ELIMINACIÓN DE ROL
        // GET: /Roles/Delete/{id}
        // ==========================================
        public IActionResult Delete(int id)
        {
            if (!EsAdmin()) return NotFound();

            var rol = _rolDAL.GetRolById(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        // ==========================================
        // ELIMINAR ROL CONFIRMADO
        // POST: /Roles/DeleteConfirmed
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!EsAdmin()) return NotFound();

            // No permitir eliminar si el rol está en uso
            if (_rolDAL.EstaRolEnUso(id))
            {
                TempData["ErrorMsg"] = "No puedes eliminar este rol porque está en uso por uno o más usuarios.";
                return RedirectToAction("Index");
            }

            _rolDAL.DeleteRol(id);
            return RedirectToAction("Index");
        }
    }
}
