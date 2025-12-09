using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Usuarios;

public class EditModel(IUsuarioService usuarioService) : PageModel
{
 private readonly IUsuarioService _service = usuarioService;

 [BindProperty]
 public Usuario Usuario { get; set; } = new();

 public async Task<IActionResult> OnGetAsync(string id)
 {
 var usuarios = await _service.GetUsuariosAsync();
 Usuario = usuarios.FirstOrDefault(u => u.Id == id) ?? new();
 if (Usuario.Id is null) return RedirectToPage("Index");
 return Page();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 await _service.UpdateAsync(Usuario);
 return RedirectToPage("Index");
 }
}