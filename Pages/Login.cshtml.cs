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
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                // Si debe cambiar contraseña, enviarlo
                var name = User.Identity!.Name!;
                // No tenemos usuario completo en claims, redirigir a usuarios por ahora
                return LocalRedirect(Url.Content("/Usuarios/Index"));
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                AlertService.Warning(TempData, "Ingrese usuario y contraseña.");
                return Page();
            }

            var usuario = await _usuarios.GetByNombreAsync(Username) ?? await _usuarios.GetByEmailAsync(Username);
            if (usuario is null)
            {
                AlertService.Error(TempData, "Usuario o contraseña inválidos.");
                return Page();
            }

            var ok = await _usuarios.VerifyPasswordAsync(usuario, Password);
            if (!ok)
            {
                AlertService.Error(TempData, "Usuario o contraseña inválidos.");
                return Page();
            }

            // Si debe cambiar contraseña, redirigir al cambio con claims mínimos
            if (usuario.mustChangePassword)
            {
                var tempClaims = new List<Claim> { new Claim(ClaimTypes.Name, usuario.nombre ?? Username!) };
                var identityTmp = new ClaimsIdentity(tempClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identityTmp));
                return LocalRedirect(Url.Content("/Account/ChangePassword"));
            }

            var fullClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.nombre ?? Username!)
            };
            if (usuario.rol ==1)
            {
                fullClaims.Add(new Claim(ClaimTypes.Role, "Administrador"));
            }

            var identity = new ClaimsIdentity(fullClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (usuario.rol ==1)
            {
                AlertService.Success(TempData, $"Bienvenido, {usuario.nombre ?? Username!}");
                return LocalRedirect(Url.Content("/Admin/Index"));
            }
            else
            {
                AlertService.Success(TempData, $"Bienvenido, {usuario.nombre ?? Username!}");
                return LocalRedirect(Url.Content("/Account/Profile"));
            }
        }
    }
}