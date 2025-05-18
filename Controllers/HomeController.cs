using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.Models;
using DameChanceSV2.DAL;
using DameChanceSV2.Utilities;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DameChanceSV2.Controllers
{
    public class HomeController : Controller
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS A TRAVÉS DEL CONSTRUCTOR
        // Incluye acceso a usuarios, perfiles, matches, mensajes, reportes y roles.
        // ==========================================
        private readonly ILogger<HomeController> _logger;
        private readonly UsuarioDAL _usuarioDAL;
        private readonly PerfilDeUsuarioDAL _perfilDal;
        private readonly MatchesDAL _matchesDAL;
        private readonly MensajeDAL _mensajeDal;
        private readonly ReporteDAL _reporteDAL;
        private readonly RolDAL _rolDAL;

        public HomeController(
            ILogger<HomeController> logger,
            UsuarioDAL usuarioDAL,
            PerfilDeUsuarioDAL perfilDal,
            MatchesDAL matchesDAL,
            MensajeDAL mensajeDAL,
            ReporteDAL reporteDAL,
            RolDAL rolDAL
        )
        {
            _logger = logger;
            _usuarioDAL = usuarioDAL;
            _perfilDal = perfilDal;
            _matchesDAL = matchesDAL;
            _mensajeDal = mensajeDAL;
            _reporteDAL = reporteDAL;
            _rolDAL = rolDAL;
        }

        // ==========================================
        // VISTAS GENERALES (Inicio, Privacidad, Error)
        // ==========================================

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ==========================================
        // VISTA DASHBOARD DE ADMINISTRADOR
        // ==========================================

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            if (!EsAdmin()) return NotFound();

            // Obtener métricas para el dashboard
            var (total, verificados, noVerificados, admins) = _usuarioDAL.GetUserCounts();
            var listaUsuarios = _usuarioDAL.GetAllUsuarios();
            int sinVerificarMas3Dias = _usuarioDAL.GetUnverifiedCountOlderThan3Days();
            int registradosHoy = _usuarioDAL.GetUsuariosRegistradosHoy();
            int totalReportes = _reporteDAL.GetTotalReportes();

            // Asignar datos a ViewBag
            ViewBag.RegistradosHoy = registradosHoy;
            ViewBag.Total = total;
            ViewBag.Verificados = verificados;
            ViewBag.NoVerificados = noVerificados;
            ViewBag.Admins = admins;
            ViewBag.SinVerificarMas3Dias = sinVerificarMas3Dias;
            ViewBag.TotalReportes = totalReportes;

            return View("AdminDashboard", listaUsuarios);
        }

        // ==========================================
        // CREAR USUARIO DESDE EL PANEL DE ADMIN
        // ==========================================

        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!EsAdmin()) return NotFound();

            var roles = _rolDAL.GetRoles();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(Usuario model)
        {
            if (!EsAdmin()) return NotFound();

            if (ModelState.IsValid)
            {
                model.Contrasena = PasswordHelper.HashPassword(model.Contrasena);
                _usuarioDAL.InsertUsuario(model);
                return RedirectToAction("AdminDashboard");
            }

            return View(model);
        }

        // ==========================================
        // EDITAR USUARIO DESDE EL PANEL DE ADMIN
        // ==========================================

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!EsAdmin()) return NotFound();

            var usuario = _usuarioDAL.GetUsuarioById(id);
            if (usuario == null) return NotFound();

            var roles = _rolDAL.GetRoles();
            ViewBag.Roles = new SelectList(roles, "Id", "Nombre", usuario.RolId);

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(Usuario model)
        {
            if (!EsAdmin()) return NotFound();

            if (ModelState.IsValid)
            {
                // Preserva la contrasena existente si no se modifica
                var usuarioOriginal = _usuarioDAL.GetUsuarioById(model.Id);
                if (usuarioOriginal != null)
                {
                    model.Contrasena = usuarioOriginal.Contrasena;
                }

                _usuarioDAL.UpdateUsuario(model);
                return RedirectToAction("AdminDashboard");
            }

            // Manejo de errores si falla el modelo
            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Errors = allErrors;

            return View(model);
        }

        // ==========================================
        // ELIMINAR USUARIO DESDE EL PANEL DE ADMIN
        // ==========================================

        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!EsAdmin()) return NotFound();

            var usuario = _usuarioDAL.GetUsuarioById(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserConfirmed(int id)
        {
            if (!EsAdmin()) return NotFound();

            _usuarioDAL.DeleteUsuario(id);
            return RedirectToAction("AdminDashboard");
        }

        // ==========================================
        // DASHBOARD DEL USUARIO REGULAR
        // ==========================================

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Verificar sesión activa
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Obtener perfil del usuario
            var perfilUsuario = _perfilDal.GetPerfilByUsuarioId(userId);
            string profileImg = string.IsNullOrEmpty(perfilUsuario?.ImagenPerfil)
                ? "images/perfiles/placeholder.png"
                : perfilUsuario.ImagenPerfil;
            ViewBag.UserProfileImage = profileImg;

            // Perfiles de otros usuarios
            var perfiles = _perfilDal.GetAllOtherProfiles(userId);

            // Datos del usuario actual
            var usuario = _usuarioDAL.GetUsuarioById(userId);
            var mutuals = _matchesDAL.GetMatchesForUser(userId);
            int matchCount = mutuals.Count;
            int newMsgs = mutuals
                .Select(u => _matchesDAL.GetConversationMatchId(userId, u.Id))
                .Sum(matchId => _mensajeDal.ContarNoLeidos(userId, matchId));

            ViewBag.UserName = usuario?.Nombre ?? "Usuario";
            ViewBag.MatchCount = matchCount;
            ViewBag.NewMsgs = newMsgs;

            return View(perfiles);
        }

        // ==========================================
        // FUNCIÓN PRIVADA PARA VALIDAR SI ES ADMIN
        // ==========================================
        private bool EsAdmin()
        {
            var userSession = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(userSession)) return false;

            if (!int.TryParse(userSession, out int userId)) return false;

            var user = _usuarioDAL.GetUsuarioById(userId);
            if (user == null) return false;

            return (user.RolId == 1); // RolId 1 = Admin
        }
    }
}
