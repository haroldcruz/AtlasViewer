using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Roles
{
 public class CreateModel : PageModel
 {
 private readonly IRolService _roles;
 public CreateModel(IRolService roles) { _roles = roles; }
 [BindProperty] public Rol Rol { get; set; } = new Rol();
 public void OnGet() { }
 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 await _roles.CreateAsync(Rol);
 TempData["PopoutTitle"] = "Rol creado";
 TempData["PopoutMessage"] = Rol.nombre_rol;
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
 }
}
