using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Usuarios;

public class IndexModel(IUsuarioService usuarioService) : PageModel
{
 private readonly IUsuarioService _usuarioService = usuarioService;

 [BindProperty]
 public AtlasViewer.Models.Usuario Nuevo { get; set; } = new();

 public IList<Usuario> Usuarios { get; private set; } = [];

 public async Task OnGetAsync()
 {
 Usuarios = await _usuarioService.GetUsuariosAsync();
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 Usuarios = await _usuarioService.GetUsuariosAsync();
 return Page();
 }
 await _usuarioService.CreateAsync(Nuevo);
 return RedirectToPage();
 }
}