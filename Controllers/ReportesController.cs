using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using Azure.Core;

namespace DameChanceSV2.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ReporteDAL _reporteDAL;
        private readonly UsuarioDAL _usuarioDAL;

        public ReportesController(ReporteDAL reporteDAL, UsuarioDAL usuarioDAL)
        {
            _reporteDAL = reporteDAL;
            _usuarioDAL = usuarioDAL;
        }

        // GET: /Reportes/Reportar?idReportado=el id del usuario reportado
        [HttpGet]
        public IActionResult Reportar(int idReportado)
        {
            var model = new Reporte { IdReportado = idReportado };
            return View(model);
        }

        // POST: /Reportes/Reportar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reportar(Reporte model)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int idReportante))
                return RedirectToAction("Login", "Account");

            model.IdReportante = idReportante;
            if (ModelState.IsValid)
            {
                _reporteDAL.InsertReporte(model);
                TempData["ReportMsg"] = "Usuario reportado correctamente.";
                return RedirectToAction("Dashboard", "Home");
            }
            return View(model);
        }

        // GET: /Reportes
        [HttpGet]
        public IActionResult Index()
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            var reportes = _reporteDAL.GetAllReportes();
            return View(reportes);
        }

        // POST: /Reportes/MarcarResuelto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarResuelto(int id)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            _reporteDAL.UpdateResuelto(id, true);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarResueltoConMensaje(int id, string mensajeResolucion)
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            _reporteDAL.MarcarComoResueltoConMensaje(id, mensajeResolucion);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult MisReportes()
        {
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var reportes = _reporteDAL.GetReportesPorUsuario(userId);
            return View(reportes);
        }


    }
}
