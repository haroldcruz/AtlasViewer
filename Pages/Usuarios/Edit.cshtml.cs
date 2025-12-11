using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AtlasViewer.Pages.Usuarios;

public class EditModel : PageModel
{
 private readonly IUsuarioService _usuarios;
 private readonly IRolService _roles;
 public EditModel(IUsuarioService usuarios, IRolService roles) { _usuarios = usuarios; _roles = roles; }
 [BindProperty] public Usuario Usuario { get; set; } = new();
 public List<SelectListItem> RolesSelect { get; set; } = new();
 public async Task<IActionResult> OnGetAsync(string id)
 {
 var u = await _usuarios.GetByIdAsync(id);
 if (u is null) return NotFound();
 Usuario = u;
 var roles = await _roles.GetRolesAsync();
 RolesSelect = roles.Select(r => new SelectListItem { Text = r.nombre_rol, Value = r.id_rol.ToString(), Selected = r.id_rol == Usuario.rol }).ToList();
 return Page();
 }
 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 var roles = await _roles.GetRolesAsync();
 RolesSelect = roles.Select(r => new SelectListItem { Text = r.nombre_rol, Value = r.id_rol.ToString(), Selected = r.id_rol == Usuario.rol }).ToList();
 return Page();
 }
 
 // Obtener el usuario actual de la BD para preservar hash y mustChangePassword
 var usuarioActual = await _usuarios.GetByIdAsync(Usuario.Id!);
 if (usuarioActual != null)
 {
 Usuario.hash = usuarioActual.hash;
 Usuario.mustChangePassword = usuarioActual.mustChangePassword;
 }
 
 await _usuarios.UpdateAsync(Usuario);
 TempData["PopoutTitle"] = "Usuario actualizado";
 TempData["PopoutMessage"] = Usuario.nombre ?? string.Empty;
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
}