using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.DAL;
using DameChanceSV2.Models;
using Azure.Core;

namespace DameChanceSV2.Controllers
{
    public class ReportesController : Controller
    {
        // ==========================================
        // DEPENDENCIAS INYECTADAS
        // ==========================================
        private readonly ReporteDAL _reporteDAL;
        private readonly UsuarioDAL _usuarioDAL;

        public ReportesController(ReporteDAL reporteDAL, UsuarioDAL usuarioDAL)
        {
            _reporteDAL = reporteDAL;
            _usuarioDAL = usuarioDAL;
        }

        // ==========================================
        // MOSTRAR FORMULARIO DE REPORTE
        // GET: /Reportes/Reportar?idReportado=xx
        // ==========================================
        [HttpGet]
        public IActionResult Reportar(int idReportado)
        {
            var model = new Reporte { IdReportado = idReportado };
            return View(model);
        }

        // ==========================================
        // PROCESAR ENVÍO DEL REPORTE
        // POST: /Reportes/Reportar
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reportar(Reporte model)
        {
            // Validar si hay sesión activa
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

        // ==========================================
        // VISTA ADMINISTRATIVA DE TODOS LOS REPORTES
        // GET: /Reportes
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            // Validar si hay sesión activa
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Validar si es administrador
            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            var reportes = _reporteDAL.GetAllReportes();
            return View(reportes);
        }

        // ==========================================
        // MARCAR UN REPORTE COMO RESUELTO
        // POST: /Reportes/MarcarResuelto
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarResuelto(int id)
        {
            // Validar si hay sesión activa
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Validar si es administrador
            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            _reporteDAL.UpdateResuelto(id, true);
            return RedirectToAction("Index");
        }

        // ==========================================
        // MARCAR REPORTE COMO RESUELTO CON MENSAJE
        // POST: /Reportes/MarcarResueltoConMensaje
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarResueltoConMensaje(int id, string mensajeResolucion)
        {
            // Validar si hay sesión activa
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Validar si es administrador
            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null || usuario.RolId != 1)
                return NotFound();

            _reporteDAL.MarcarComoResueltoConMensaje(id, mensajeResolucion);
            return RedirectToAction("Index");
        }

        // ==========================================
        // VISTA DE REPORTES HECHOS POR EL USUARIO
        // GET: /Reportes/MisReportes
        // ==========================================
        [HttpGet]
        public IActionResult MisReportes()
        {
            // Validar si hay sesión activa
            if (!int.TryParse(Request.Cookies["UserSession"], out int userId))
                return RedirectToAction("Login", "Account");

            // Validar si el usuario existe
            var usuario = _usuarioDAL.GetUsuarioById(userId);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var reportes = _reporteDAL.GetReportesPorUsuario(userId);
            return View(reportes);
        }
    }
}
