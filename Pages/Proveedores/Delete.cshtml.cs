using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.Proveedores
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly IProveedorService _proveedorService;

        public DeleteModel(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [BindProperty]
        public Proveedor Proveedor { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var proveedor = await _proveedorService.GetByIdAsync(id);

            if (proveedor == null)
            {
                return NotFound();
            }

            Proveedor = proveedor;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Proveedor.Id))
            {
                return NotFound();
            }

            await _proveedorService.DeleteAsync(Proveedor.Id);
            AlertService.Success(TempData, $"Proveedor '{Proveedor.nombre}' eliminado exitosamente");
            return RedirectToPage("Index");
        }
    }
}
