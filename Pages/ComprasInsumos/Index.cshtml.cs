using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.ComprasInsumos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICompraInsumoService _compraInsumoService;
        private readonly IProveedorService _proveedorService;

        public IndexModel(ICompraInsumoService compraInsumoService, IProveedorService proveedorService)
        {
            _compraInsumoService = compraInsumoService;
            _proveedorService = proveedorService;
        }

        public List<CompraInsumo> Compras { get; set; } = new();
        public Dictionary<string, string> ProveedoresDict { get; set; } = new();

        public async Task OnGetAsync()
        {
            Compras = await _compraInsumoService.GetAllAsync();
            Compras = Compras.OrderByDescending(c => c.fecha).ToList();

            var proveedores = await _proveedorService.GetAllAsync();
            ProveedoresDict = proveedores.ToDictionary(p => p.Id!, p => p.nombre);
        }
    }
}
