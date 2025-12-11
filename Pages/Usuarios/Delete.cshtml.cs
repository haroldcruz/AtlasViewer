using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Usuarios;

public class DeleteModel(IUsuarioService usuarioService) : PageModel
{
 private readonly IUsuarioService _service = usuarioService;

 public string? Id { get; private set; }
 public string? Nombre { get; private set; }
 public string? Email { get; private set; }

 public async Task<IActionResult> OnGetAsync(string id)
 {
 var usuario = await _service.GetByIdAsync(id);
 if (usuario == null) return NotFound();
 
 Id = id;
 Nombre = usuario.nombre;
 Email = usuario.email;
 return Page();
 }

 public async Task<IActionResult> OnPostAsync(string id)
 {
 await _service.DeleteAsync(id);
 return RedirectToPage("Index");
 }
}