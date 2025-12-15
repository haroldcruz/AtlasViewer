using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.ComprasInsumos
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly ICompraInsumoService _compraInsumoService;
        private readonly IProveedorService _proveedorService;
        private readonly IInsumoService _insumoService;

        public DeleteModel(
            ICompraInsumoService compraInsumoService,
            IProveedorService proveedorService,
            IInsumoService insumoService)
        {
            _compraInsumoService = compraInsumoService;
            _proveedorService = proveedorService;
            _insumoService = insumoService;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Compra.Id))
            {
                return NotFound();
            }

            var deleted = await _compraInsumoService.DeleteAsync(Compra.Id);
            
            if (deleted)
            {
                AlertService.Warning(TempData, "Compra eliminada. Nota: El inventario NO fue revertido.");
            }
            else
            {
                AlertService.Error(TempData, "No se pudo eliminar la compra.");
            }

            return RedirectToPage("Index");
        }
    }
}
