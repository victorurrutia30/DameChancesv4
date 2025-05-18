using System.Threading.Tasks;
using DameChanceSV2.DAL;
using Microsoft.AspNetCore.Mvc;

namespace DameChanceSV2.ViewComponents
{
    // =============================================================================
    // COMPONENTE DE VISTA: CompleteProfileBannerViewComponent
    // FUNCIONALIDAD:
    // Este componente se encarga de verificar si un usuario autenticado
    // aún no ha completado su perfil. En caso afirmativo, muestra un banner
    // recordatorio (por ejemplo, en el dashboard).
    // =============================================================================
    public class CompleteProfileBannerViewComponent : ViewComponent
    {
        private readonly PerfilDeUsuarioDAL _perfilDal;

        // Constructor con inyección del DAL de perfil
        public CompleteProfileBannerViewComponent(PerfilDeUsuarioDAL perfilDal)
        {
            _perfilDal = perfilDal;
        }

        // ==========================================================
        // MÉTODO: InvokeAsync
        // TIPO: Task<IViewComponentResult>
        // FUNCIONALIDAD:
        // 1. Verifica si hay sesión iniciada (cookie).
        // 2. Valida el ID del usuario.
        // 3. Verifica si ya tiene un perfil completo.
        // 4. Si NO lo tiene, renderiza el banner de advertencia.
        // ==========================================================
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Paso 1: Obtener ID del usuario desde la cookie
            var userSession = Request.Cookies["UserSession"];
            if (string.IsNullOrEmpty(userSession))
                return Content(string.Empty); // No hay sesión -> no se muestra nada

            // Paso 2: Validar que la cookie sea un número entero (ID)
            if (!int.TryParse(userSession, out int userId))
                return Content(string.Empty); // Cookie inválida

            // Paso 3: Consultar si el usuario ya tiene un perfil registrado
            var perfil = _perfilDal.GetPerfilByUsuarioId(userId);
            if (perfil != null)
                return Content(string.Empty); // Ya tiene perfil -> no mostrar banner

            // Paso 4: Mostrar banner parcial (vista asociada al componente)
            return View();
        }
    }
}
