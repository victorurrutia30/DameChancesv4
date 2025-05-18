using Microsoft.AspNetCore.Mvc;
using DameChanceSV2.Models;
using DameChanceSV2.DAL;
using DameChanceSV2.Utilities;
using System;
using DameChanceSV2.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DameChanceSV2.Controllers
{
    public class AccountController : Controller
    {
        // =========================================
        // DEPENDENCIAS Y CAMPOS PRIVADOS
        // =========================================
        private readonly UsuarioDAL _usuarioDAL;
        private readonly IEmailService _emailService;
        private readonly PerfilDeUsuarioDAL _perfilDeUsuarioDAL;

        // Almacenamiento temporal para tokens de verificación de cuenta y reseteo de contrasena.
        private static Dictionary<string, int> EmailVerificationTokens = new Dictionary<string, int>();
        private static Dictionary<string, int> PasswordResetTokens = new Dictionary<string, int>();

        // =========================================
        // CONSTRUCTOR - Inyección de dependencias
        // =========================================
        public AccountController(UsuarioDAL usuarioDAL, IEmailService emailService, PerfilDeUsuarioDAL perfilDeUsuarioDAL)
        {
            _usuarioDAL = usuarioDAL;
            _emailService = emailService;
            _perfilDeUsuarioDAL = perfilDeUsuarioDAL;
        }

        // =========================================
        // REGISTRO DE USUARIO (GET/POST)
        // =========================================

        // Muestra el formulario de registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        // Procesa el registro del usuario, genera token y envía correo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuarioExistente = _usuarioDAL.GetUsuarioByCorreo(model.Correo);
                if (usuarioExistente != null)
                {
                    ModelState.AddModelError("Correo", "El correo ya está registrado.");
                    return View(model);
                }

                Usuario nuevoUsuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Correo = model.Correo,
                    Contrasena = PasswordHelper.HashPassword(model.Contrasena),
                    Estado = false,
                    RolId = 2
                };

                int nuevoId = _usuarioDAL.InsertUsuario(nuevoUsuario);

                string token = Guid.NewGuid().ToString();
                EmailVerificationTokens[token] = nuevoId;

                var verificationLink = Url.Action("VerificarCuenta", "Account", new { token = token }, Request.Scheme);
                string subject = "Verifica tu cuenta en DameChance";
                string body = $"<p>Hola {model.Nombre},</p><p>Verifica tu cuenta haciendo clic en:</p><a href='{verificationLink}'>Verificar mi cuenta</a>";

                _emailService.SendEmail(model.Correo, subject, body);

                ViewBag.Message = "Registro exitoso. Se ha enviado un correo de verificación.";
                return View("Informacion");
            }
            return View(model);
        }

        // =========================================
        // VERIFICAR CUENTA POR TOKEN (GET)
        // =========================================
        [HttpGet]
        public IActionResult VerificarCuenta(string token)
        {
            if (string.IsNullOrEmpty(token) || !EmailVerificationTokens.ContainsKey(token))
            {
                ViewBag.ErrorMessage = "El token es inválido o ha expirado.";
                return View("Error");
            }

            int usuarioId = EmailVerificationTokens[token];
            _usuarioDAL.UpdateEstado(usuarioId, true);
            EmailVerificationTokens.Remove(token);

            ViewBag.Message = "Cuenta verificada con éxito. Ahora puedes iniciar sesión.";
            return View();
        }

        // =========================================
        // LOGIN (GET/POST)
        // =========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = _usuarioDAL.GetUsuarioByCorreo(model.Correo);
                if (usuario != null && PasswordHelper.VerifyPassword(model.Contrasena, usuario.Contrasena))
                {
                    if (!usuario.Estado)
                    {
                        ModelState.AddModelError("", "Tu cuenta no está verificada.");
                        return View(model);
                    }

                    var options = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.Now.AddMinutes(30)
                    };

                    Response.Cookies.Append("UserSession", usuario.Id.ToString(), options);
                    Response.Cookies.Append("UserRole", usuario.RolId.ToString(), options);

                    if (usuario.RolId != 1)
                    {
                        var perfil = _perfilDeUsuarioDAL.GetPerfilByUsuarioId(usuario.Id);
                        if (perfil == null)
                            return RedirectToAction("CompletarPerfil", "Account");
                    }

                    return usuario.RolId == 1
                        ? RedirectToAction("AdminDashboard", "Home")
                        : RedirectToAction("Dashboard", "Home");
                }

                ModelState.AddModelError(string.Empty, "Correo o password incorrectos.");
            }
            return View(model);
        }

        // =========================================
        // LOGOUT (GET)
        // =========================================

        public IActionResult Logout()
        {
            Response.Cookies.Delete("UserSession");
            Response.Cookies.Delete("UserRole");
            return RedirectToAction("Login");
        }

        // =========================================
        // RECUPERAR CONTRASEnA (GET/POST)
        // =========================================

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarContrasena(RecuperarContrasenaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = _usuarioDAL.GetUsuarioByCorreo(model.Correo);
                if (usuario != null)
                {
                    string token = Guid.NewGuid().ToString();
                    PasswordResetTokens[token] = usuario.Id;

                    var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
                    string subject = "Recuperación de password - DameChance";
                    string body = $"<p>Hola {usuario.Nombre},</p><p>Recibimos una solicitud para restablecer tu password.</p><a href='{resetLink}'>Recuperar Password</a>";

                    _emailService.SendEmail(usuario.Correo, subject, body);

                    ViewBag.Message = "Se ha enviado un enlace de recuperación a tu correo.";
                }
                else
                {
                    ModelState.AddModelError("Correo", "El correo no se encuentra registrado.");
                }
            }
            return View(model);
        }

        // =========================================
        // RESET PASSWORD POR TOKEN (GET/POST)
        // =========================================

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string token, string nuevaContrasena, string confirmarContrasena)
        {
            if (string.IsNullOrEmpty(nuevaContrasena) || string.IsNullOrEmpty(confirmarContrasena))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                ViewBag.Token = token;
                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ModelState.AddModelError("", "Las passwords no coinciden.");
                ViewBag.Token = token;
                return View();
            }

            if (!PasswordResetTokens.ContainsKey(token))
            {
                ModelState.AddModelError("", "El token es inválido o ha expirado.");
                ViewBag.Token = token;
                return View("ResetPassword");
            }

            int userId = PasswordResetTokens[token];
            string hashed = PasswordHelper.HashPassword(nuevaContrasena);
            _usuarioDAL.UpdateContrasena(userId, hashed);
            PasswordResetTokens.Remove(token);

            return RedirectToAction("Login");
        }

        // =========================================
        // COMPLETAR PERFIL (GET/POST)
        // =========================================

        [HttpGet]
        public IActionResult CompletarPerfil()
        {
            var userSession = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(userSession))
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompletarPerfil(CompletarPerfilViewModel model)
        {
            var userSession = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(userSession) || !int.TryParse(userSession, out int userId))
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var perfilExistente = _perfilDeUsuarioDAL.GetPerfilByUsuarioId(userId);
                string rutaImagen = null;

                if (model.ImagenPerfil != null && model.ImagenPerfil.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(model.ImagenPerfil.FileName);
                    string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "perfiles");
                    string rutaFisica = Path.Combine(carpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        model.ImagenPerfil.CopyTo(stream);
                    }

                    rutaImagen = Path.Combine("images", "perfiles", nombreArchivo).Replace("\\", "/");
                }

                if (perfilExistente == null)
                {
                    PerfilDeUsuario nuevoPerfil = new PerfilDeUsuario
                    {
                        UsuarioId = userId,
                        Edad = model.Edad,
                        Genero = model.Genero,
                        Intereses = model.Intereses,
                        Ubicacion = model.Ubicacion,
                        ImagenPerfil = rutaImagen
                    };
                    _perfilDeUsuarioDAL.InsertPerfil(nuevoPerfil);
                }
                else
                {
                    perfilExistente.Edad = model.Edad;
                    perfilExistente.Genero = model.Genero;
                    perfilExistente.Intereses = model.Intereses;
                    perfilExistente.Ubicacion = model.Ubicacion;
                    if (!string.IsNullOrEmpty(rutaImagen))
                    {
                        perfilExistente.ImagenPerfil = rutaImagen;
                    }
                    _perfilDeUsuarioDAL.UpdatePerfil(perfilExistente);
                }

                return RedirectToAction("Dashboard", "Home");
            }

            return View(model);
        }
    }
}
