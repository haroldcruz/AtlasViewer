using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AtlasViewer.Pages.Usuarios;

public class CreateModel : PageModel
{
 private readonly IUsuarioService _service;
 private readonly IRolService _rolesService;
 public CreateModel(IUsuarioService service, IRolService rolesService) { _service = service; _rolesService = rolesService; }

 [BindProperty]
 public Usuario Usuario { get; set; } = new Usuario();
 public List<SelectListItem> RolesSelect { get; set; } = new();

 public async Task OnGetAsync()
 {
 var roles = await _rolesService.GetRolesAsync();
 RolesSelect = roles.Select(r => new SelectListItem { Text = r.nombre_rol, Value = r.id_rol.ToString() }).ToList();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 await OnGetAsync();
 return Page();
 }
 // Asignar contrase�a temporal y generar hash
 var tempPassword = "abc123";
 var hash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
 Usuario.hash = hash;
 Usuario.mustChangePassword = true; // obliga cambio al primer login
 // Guardar
 await _service.CreateAsync(Usuario);
 AlertService.Success(TempData, $"Usuario '{Usuario.nombre}' creado exitosamente. Contraseña temporal asignada (debe cambiarse en el primer ingreso).");
 return RedirectToPage("Index");
 }
}