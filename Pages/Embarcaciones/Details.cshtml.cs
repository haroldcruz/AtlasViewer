using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Embarcaciones;

public class DetailsModel : PageModel
{
    private readonly IEmbarcacionService _embarcacionService;
    private readonly IPescadorService _pescadorService;

    public DetailsModel(IEmbarcacionService embarcacionService, IPescadorService pescadorService)
    {
        _embarcacionService = embarcacionService;
        _pescadorService = pescadorService;
    }

    public Embarcacion Embarcacion { get; set; } = new();
    public Pescador? Pescador { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var embarcacion = await _embarcacionService.GetByIdAsync(id);
        if (embarcacion == null)
        {
            return NotFound();
        }

        Embarcacion = embarcacion;
        
        if (!string.IsNullOrEmpty(embarcacion.pescadorId))
        {
            Pescador = await _pescadorService.GetByIdAsync(embarcacion.pescadorId);
        }

        return Page();
    }
}
