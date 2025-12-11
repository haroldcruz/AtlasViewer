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
 TempData["PopoutTitle"] = "No se puede eliminar";
 TempData["PopoutMessage"] = "Este rol tiene usuarios asignados y no puede ser eliminado.";
 TempData["PopoutIcon"] = "error";
 return RedirectToPage("Index");
 }
 TempData["PopoutTitle"] = "Rol eliminado";
 TempData["PopoutMessage"] = Rol.nombre_rol;
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
 }
}
