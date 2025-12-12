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
 AlertService.Error(TempData, "Las contraseñas no coinciden.");
 return Page();
 }
 var name = User.Identity?.Name;
 var usuario = await _usuarios.GetByNombreAsync(name!) ?? await _usuarios.GetByEmailAsync(name!);
 if (usuario is null)
 {
 AlertService.Error(TempData, "Usuario no encontrado.");
 return Page();
 }
 var ok = await _usuarios.VerifyPasswordAsync(usuario, Current);
 if (!ok)
 {
 AlertService.Error(TempData, "Contraseña actual incorrecta.");
 return Page();
 }
 usuario.hash = BCrypt.Net.BCrypt.HashPassword(New);
 usuario.mustChangePassword = false;
 await _usuarios.UpdateAsync(usuario);
 AlertService.Success(TempData, "Contraseña actualizada exitosamente.");
 return LocalRedirect(Url.Content("~/"));
 }
 }
}
