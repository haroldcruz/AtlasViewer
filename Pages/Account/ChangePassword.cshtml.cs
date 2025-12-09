using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AtlasViewer.Pages.Account
{
 [Authorize]
 public class ChangePasswordModel : PageModel
 {
 private readonly IUsuarioService _usuarios;
 public ChangePasswordModel(IUsuarioService usuarios) { _usuarios = usuarios; }
 [BindProperty] public string Current { get; set; } = string.Empty;
 [BindProperty] public string New { get; set; } = string.Empty;
 [BindProperty] public string Confirm { get; set; } = string.Empty;
 public void OnGet() { }
 public async Task<IActionResult> OnPostAsync()
 {
 if (string.IsNullOrWhiteSpace(New) || New != Confirm)
 {
 TempData["PopoutTitle"] = "Cambiar contraseña";
 TempData["PopoutMessage"] = "Las contraseñas no coinciden.";
 TempData["PopoutIcon"] = "error";
 return Page();
 }
 var name = User.Identity?.Name;
 var usuario = await _usuarios.GetByNombreAsync(name!) ?? await _usuarios.GetByEmailAsync(name!);
 if (usuario is null)
 {
 TempData["PopoutTitle"] = "Cambiar contraseña";
 TempData["PopoutMessage"] = "Usuario no encontrado.";
 TempData["PopoutIcon"] = "error";
 return Page();
 }
 var ok = await _usuarios.VerifyPasswordAsync(usuario, Current);
 if (!ok)
 {
 TempData["PopoutTitle"] = "Cambiar contraseña";
 TempData["PopoutMessage"] = "Contraseña actual incorrecta.";
 TempData["PopoutIcon"] = "error";
 return Page();
 }
 usuario.hash = BCrypt.Net.BCrypt.HashPassword(New);
 usuario.mustChangePassword = false;
 await _usuarios.UpdateAsync(usuario);
 TempData["PopoutTitle"] = "Contraseña actualizada";
 TempData["PopoutMessage"] = "Ahora puede continuar";
 TempData["PopoutIcon"] = "success";
 return LocalRedirect(Url.Content("~/"));
 }
 }
}
