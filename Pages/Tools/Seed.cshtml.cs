using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Tools
{
 public class SeedModel : PageModel
 {
 private readonly IUsuarioService _usuarios;
 private readonly IWebHostEnvironment _env;
 public SeedModel(IUsuarioService usuarios, IWebHostEnvironment env)
 {
 _usuarios = usuarios; _env = env;
 }

 [BindProperty] public string Nombre { get; set; } = "admin";
 [BindProperty] public string Email { get; set; } = "admin@example.com";
 [BindProperty] public string Password { get; set; } = "123456";
 public string? HashGenerado { get; set; }
 public string? ResultMessage { get; set; }
 public bool IsDevelopment => _env.IsDevelopment();

 public void OnGet() { }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!IsDevelopment)
 {
 return Forbid();
 }
 if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
 {
 ResultMessage = "Complete todos los campos";
 return Page();
 }
 // Generar hash y crear/actualizar
 var hash = BCrypt.Net.BCrypt.HashPassword(Password);
 HashGenerado = hash;
 // Buscar si existe por email o nombre
 var u = await _usuarios.GetByEmailAsync(Email) ?? await _usuarios.GetByNombreAsync(Nombre);
 if (u is null)
 {
 u = new Usuario { nombre = Nombre, email = Email, hash = hash, rol =1 };
 await _usuarios.CreateAsync(u);
 ResultMessage = "Usuario creado con rol Administrador.";
 }
 else
 {
 u.nombre = Nombre; u.email = Email; u.hash = hash; u.rol =1;
 await _usuarios.UpdateAsync(u);
 ResultMessage = "Usuario actualizado con nuevo hash y rol Administrador.";
 }
 AlertService.Success(TempData, $"Usuario '{Nombre}' listo para login.");
 return Page();
 }
 }
}
