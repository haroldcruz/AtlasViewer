using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Account
{
 [Authorize]
 public class ProfileModel : PageModel
 {
 private readonly IUsuarioService _usuarios;
 public ProfileModel(IUsuarioService usuarios) { _usuarios = usuarios; }
 [BindProperty] public string Nombre { get; set; } = string.Empty;
 [BindProperty] public string Email { get; set; } = string.Empty;
 public async Task OnGetAsync()
 {
 var name = User.Identity?.Name;
 var usuario = await _usuarios.GetByNombreAsync(name!) ?? await _usuarios.GetByEmailAsync(name!);
 if (usuario is not null) { Nombre = usuario.nombre ?? string.Empty; Email = usuario.email ?? string.Empty; }
 }
 public async Task<IActionResult> OnPostAsync()
 {
 var name = User.Identity?.Name;
 var usuario = await _usuarios.GetByNombreAsync(name!) ?? await _usuarios.GetByEmailAsync(name!);
 if (usuario is null) return Page();
 usuario.nombre = Nombre; usuario.email = Email;
 await _usuarios.UpdateAsync(usuario);
 TempData["PopoutTitle"] = "Perfil actualizado";
 TempData["PopoutMessage"] = "Sus datos han sido guardados";
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("/Usuarios/Index");
 }
 }
}
