using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.Proveedores
{
    [Authorize(Roles = "Administrador,Editor")]
    public class CreateModel : PageModel
    {
        private readonly IProveedorService _proveedorService;

        public CreateModel(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [BindProperty]
        public Proveedor Proveedor { get; set; } = new Proveedor { activo = true };

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _proveedorService.CreateAsync(Proveedor);
            AlertService.Success(TempData, $"Proveedor '{Proveedor.nombre}' creado exitosamente");
            return RedirectToPage("Index");
        }
    }
}
