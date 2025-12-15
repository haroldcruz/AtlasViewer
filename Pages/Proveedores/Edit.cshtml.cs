using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.Proveedores
{
    [Authorize(Roles = "Administrador,Editor")]
    public class EditModel : PageModel
    {
        private readonly IProveedorService _proveedorService;

        public EditModel(IProveedorService proveedorService)
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _proveedorService.UpdateAsync(Proveedor.Id!, Proveedor);
            AlertService.Success(TempData, $"Proveedor '{Proveedor.nombre}' actualizado exitosamente");
            return RedirectToPage("Index");
        }
    }
}
