using AtlasViewer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AtlasViewer.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUsuarioService _usuarios;
        public LoginModel(IUsuarioService usuarios) { _usuarios = usuarios; }

        [BindProperty]
        public string? Username { get; set; }
        [BindProperty]
        public string? Password { get; set; }

        public IActionResult OnGet()
        {
            // Si ya está autenticado, redirigir a la página adecuada según rol
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole("Administrador"))
                {
                    return LocalRedirect(Url.Content("/Admin/Index"));
                }
                return LocalRedirect(Url.Content("/Usuarios/Index"));
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["PopoutTitle"] = "Inicio de sesión";
                TempData["PopoutMessage"] = "Ingrese usuario y contraseña.";
                TempData["PopoutIcon"] = "warning";
                return Page();
            }

            // Buscar por nombre primero; si no existe, probar por email
            var usuario = await _usuarios.GetByNombreAsync(Username) ?? await _usuarios.GetByEmailAsync(Username);
            if (usuario is null)
            {
                TempData["PopoutTitle"] = "Inicio de sesión";
                TempData["PopoutMessage"] = "Usuario o contraseña inválidos.";
                TempData["PopoutIcon"] = "error";
                return Page();
            }

            var ok = await _usuarios.VerifyPasswordAsync(usuario, Password);
            if (!ok)
            {
                TempData["PopoutTitle"] = "Inicio de sesión";
                TempData["PopoutMessage"] = "Usuario o contraseña inválidos.";
                TempData["PopoutIcon"] = "error";
                return Page();
            }

            // Construir claims: nombre y rol básico. Ajusta para cargar roles reales si existen.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.nombre ?? Username!)
            };
            // Si el modelo tiene un campo 'rol' numérico, mapear '1' a Administrador
            if (usuario.rol ==1)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirigir según rol
            if (usuario.rol ==1)
            {
                TempData["PopoutTitle"] = "Bienvenido";
                TempData["PopoutMessage"] = usuario.nombre ?? Username!;
                TempData["PopoutIcon"] = "success";
                return LocalRedirect(Url.Content("/Admin/Index"));
            }
            else
            {
                TempData["PopoutTitle"] = "Bienvenido";
                TempData["PopoutMessage"] = usuario.nombre ?? Username!;
                TempData["PopoutIcon"] = "success";
                return LocalRedirect(Url.Content("/Usuarios/Index"));
            }
        }
    }
}
