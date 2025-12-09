using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Usuarios;

public class CreateModel(IUsuarioService usuarioService) : PageModel
{
 private readonly IUsuarioService _service = usuarioService;

 [BindProperty]
 public Usuario Usuario { get; set; } = new();

 public void OnGet() {}

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid) return Page();
 // Asignar contraseña temporal y generar hash
 var tempPassword = "abc123";
 var hash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
 Usuario.hash = hash;
 // Guardar
 await _service.CreateAsync(Usuario);
 TempData["PopoutTitle"] = "Usuario creado";
 TempData["PopoutMessage"] = $"Se asignó una contraseña temporal. Debe cambiarse en el primer ingreso.";
 TempData["PopoutIcon"] = "success";
 return RedirectToPage("Index");
 }
}