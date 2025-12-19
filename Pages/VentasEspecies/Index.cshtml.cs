using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.VentasEspecies
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IVentaEspecieService _ventaEspecieService;

        public IndexModel(IVentaEspecieService ventaEspecieService)
        {
            _ventaEspecieService = ventaEspecieService;
        }

        public List<VentaEspecie> Ventas { get; set; } = new();

        public async Task OnGetAsync()
        {
            Ventas = await _ventaEspecieService.GetAllAsync();
            Ventas = Ventas.OrderByDescending(v => v.fecha).ToList();
        }
    }
}
