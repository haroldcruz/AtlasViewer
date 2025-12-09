using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Usuarios;

public class DetailsModel(IUsuarioService usuarioService) : PageModel
{
 private readonly IUsuarioService _service = usuarioService;

 public Usuario Usuario { get; private set; } = new();

 public async Task OnGetAsync(string id)
 {
 var usuarios = await _service.GetUsuariosAsync();
 Usuario = usuarios.FirstOrDefault(u => u.Id == id) ?? new();
 }
}