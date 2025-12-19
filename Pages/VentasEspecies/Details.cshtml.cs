using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.VentasEspecies
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IVentaEspecieService _ventaEspecieService;
        private readonly IEspecieService _especieService;

        public DetailsModel(IVentaEspecieService ventaEspecieService, IEspecieService especieService)
        {
            _ventaEspecieService = ventaEspecieService;
            _especieService = especieService;
        }

        public VentaEspecie VentaEspecie { get; set; } = new();
        public Dictionary<string, string> EspeciesDict { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            VentaEspecie = await _ventaEspecieService.GetByIdAsync(id);

            if (VentaEspecie == null)
            {
                return NotFound();
            }

            var especies = await _especieService.GetAllAsync();
            EspeciesDict = especies.ToDictionary(e => e.Id!, e => $"{e.codigo} - {e.nombre}");

            return Page();
        }
    }
}
