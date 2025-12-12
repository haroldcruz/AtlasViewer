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
 AlertService.Success(TempData, $"Rol '{Rol.nombre_rol}' creado exitosamente.");
 return RedirectToPage("Index");
 }
 }
}
