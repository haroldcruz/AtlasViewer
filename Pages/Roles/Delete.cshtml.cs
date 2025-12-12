using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Roles
{
 public class DeleteModel : PageModel
 {
 private readonly IRolService _roles;
 private readonly IUsuarioService _usuarios;
 public DeleteModel(IRolService roles, IUsuarioService usuarios) { _roles = roles; _usuarios = usuarios; }
 [BindProperty] public Rol Rol { get; set; } = new Rol();
 public async Task<IActionResult> OnGetAsync(string id)
 {
 var r = await _roles.GetByIdAsync(id);
 if (r is null) return NotFound();
 Rol = r; return Page();
 }
 public async Task<IActionResult> OnPostAsync()
 {
 var eliminado = await _roles.DeleteAsync(Rol.Id!, _usuarios);
 if (!eliminado)
 {
 AlertService.Error(TempData, "No se puede eliminar este rol porque tiene usuarios asignados.");
 return RedirectToPage("Index");
 }
 AlertService.Success(TempData, $"Rol '{Rol.nombre_rol}' eliminado exitosamente.");
 return RedirectToPage("Index");
 }
 }
}
