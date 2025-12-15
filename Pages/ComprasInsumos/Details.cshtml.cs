using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.ComprasInsumos
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ICompraInsumoService _compraInsumoService;
        private readonly IProveedorService _proveedorService;
        private readonly IInsumoService _insumoService;

        public DetailsModel(
            ICompraInsumoService compraInsumoService,
            IProveedorService proveedorService,
            IInsumoService insumoService)
        {
            _compraInsumoService = compraInsumoService;
            _proveedorService = proveedorService;
            _insumoService = insumoService;
        }

        public CompraInsumo Compra { get; set; } = null!;
        public Proveedor? Proveedor { get; set; }
        public Dictionary<string, Insumo> InsumosDict { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var compra = await _compraInsumoService.GetByIdAsync(id);
            if (compra == null)
            {
                return NotFound();
            }

            Compra = compra;

            // Cargar proveedor
            if (!string.IsNullOrEmpty(Compra.proveedorId))
            {
                Proveedor = await _proveedorService.GetByIdAsync(Compra.proveedorId);
            }

            // Cargar insumos
            var insumos = await _insumoService.GetAllAsync();
            InsumosDict = insumos.Where(i => i.Id != null).ToDictionary(i => i.Id!, i => i);

            return Page();
        }
    }
}
