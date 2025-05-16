using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.Models;
using DameChanceSV2.DAL;
using DameChanceSV2.Utilities;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DameChanceSV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsuarioDAL _usuarioDAL;
        private readonly PerfilDeUsuarioDAL _perfilDal;
        private readonly MatchesDAL _matchesDAL;      
        private readonly MensajeDAL _mensajeDal;

        public HomeController(ILogger<HomeController> logger, UsuarioDAL usuarioDAL, PerfilDeUsuarioDAL perfilDal, MatchesDAL matchesDAL, MensajeDAL mensajeDAL)
        {
            _logger = logger;
            _usuarioDAL = usuarioDAL;
            _perfilDal = perfilDal;
            _matchesDAL = matchesDAL;      
            _mensajeDal = mensajeDAL;
        }

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

        // =====================================
        // ADMIN DASHBOARD
        // =====================================
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            if (!EsAdmin()) return NotFound();

            var (total, verificados, noVerificados, admins) = _usuarioDAL.GetUserCounts();
            var listaUsuarios = _usuarioDAL.GetAllUsuarios();
            int sinVerificarMas3Dias = _usuarioDAL.GetUnverifiedCountOlderThan3Days();

            ViewBag.Total = total;
            ViewBag.Verificados = verificados;
            ViewBag.NoVerificados = noVerificados;
            ViewBag.Admins = admins;
            ViewBag.SinVerificarMas3Dias = sinVerificarMas3Dias;

            return View("AdminDashboard", listaUsuarios);
        }

        // GET: /Home/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!EsAdmin()) return NotFound();
            return View();
        }

        // POST: /Home/CreateUser
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

        // GET: /Home/EditUser/
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!EsAdmin()) return NotFound();

            var usuario = _usuarioDAL.GetUsuarioById(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: /Home/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(Usuario model)
        {
            if (!EsAdmin()) return NotFound();

            if (ModelState.IsValid)
            {
                // Recuperar la contraseña original del usuario
                var usuarioOriginal = _usuarioDAL.GetUsuarioById(model.Id);
                if (usuarioOriginal != null)
                {
                    model.Contrasena = usuarioOriginal.Contrasena;
                }

                _usuarioDAL.UpdateUsuario(model);
                return RedirectToAction("AdminDashboard");
            }

            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Errors = allErrors;

            return View(model);
        }

        // GET: /Home/DeleteUser/
        [HttpGet]
        public IActionResult DeleteUser(int id)
        {
            if (!EsAdmin()) return NotFound();

            var usuario = _usuarioDAL.GetUsuarioById(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: /Home/DeleteUserConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserConfirmed(int id)
        {
            if (!EsAdmin()) return NotFound();

            _usuarioDAL.DeleteUsuario(id);
            return RedirectToAction("AdminDashboard");
        }

        // =====================================
        // MÉTODO PRIVADO DE CHEQUEO DE ADMIN
        // =====================================
        private bool EsAdmin()
        {
            var userSession = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(userSession)) return false;

            if (!int.TryParse(userSession, out int userId)) return false;

            var user = _usuarioDAL.GetUsuarioById(userId);
            if (user == null) return false;

            return (user.RolId == 1); // Rol 1 = Admin
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // Validar sesión
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Perfil usuario actual
            var perfilUsuario = _perfilDal.GetPerfilByUsuarioId(userId);

            // Determinar ruta de imagen
            string profileImg = string.IsNullOrEmpty(perfilUsuario?.ImagenPerfil)
                ? "images/perfiles/placeholder.png"
                : perfilUsuario.ImagenPerfil;
            ViewBag.UserProfileImage = profileImg;

            
            var perfiles = _perfilDal.GetAllOtherProfiles(userId);

            var usuario = _usuarioDAL.GetUsuarioById(userId);

            var mutuals = _matchesDAL.GetMatchesForUser(userId);
            int matchCount = mutuals.Count;
            int newMsgs = mutuals
                .Select(u => _matchesDAL.GetConversationMatchId(userId, u.Id))
                .Sum(matchId => _mensajeDal.ContarNoLeidos(userId, matchId));

            ViewBag.UserName = usuario?.Nombre ?? "Usuario";
            ViewBag.MatchCount = matchCount;
            ViewBag.NewMsgs = newMsgs;

            return View(perfiles); ViewBag.UserName = usuario?.Nombre ?? "Usuario";
            ViewBag.MatchCount = matchCount;
            ViewBag.NewMsgs = newMsgs;

            return View(perfiles);
        }
    }
}
