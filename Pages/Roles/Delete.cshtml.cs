using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Roles
{
 public class DeleteModel : PageModel
 {
 private readonly IRolService _roles;
 public DeleteModel(IRolService roles) { _roles = roles; }
 [BindProperty] public Rol Rol { get; set; } = new Rol();
 public async Task<IActionResult> OnGetAsync(string id)
 {
 var r = await _roles.GetByIdAsync(id);
 if (r is null) return NotFound();
 Rol = r; return Page();
 }
 public async Task<IActionResult> OnPostAsync()
 {
 await _roles.DeleteAsync(Rol.Id!);
 TempData["PopoutTitle"] = "Rol eliminado";
 TempData["PopoutMessage"] = Rol.nombre_rol;
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
 }
}
