using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages
{
 public class LogoutModel : PageModel
 {
 public async Task<IActionResult> OnPostAsync()
 {
 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 // Limpiar TempData por si quedara algún mensaje
 TempData.Clear();
 // Redirigir a Login
 return LocalRedirect(Url.Content("/Login"));
 }
 }
}
