using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Roles
{
 public class EditModel : PageModel
 {
 private readonly IRolService _roles;
 public EditModel(IRolService roles) { _roles = roles; }
 [BindProperty] public Rol Rol { get; set; } = new Rol();
 public async Task<IActionResult> OnGetAsync(string id)
 {
 var r = await _roles.GetByIdAsync(id);
 if (r is null) return NotFound();
 Rol = r; return Page();
 }
 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 await _roles.UpdateAsync(Rol);
 TempData["PopoutTitle"] = "Rol actualizado";
 TempData["PopoutMessage"] = Rol.nombre;
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
 }
}
