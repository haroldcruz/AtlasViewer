using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.Proveedores
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IProveedorService _proveedorService;

        public DetailsModel(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

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
    }
}
