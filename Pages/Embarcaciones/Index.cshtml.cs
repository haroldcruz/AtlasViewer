using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Embarcaciones;

public class IndexModel : PageModel
{
    private readonly IEmbarcacionService _embarcacionService;
    private readonly IPescadorService _pescadorService;

    public IndexModel(IEmbarcacionService embarcacionService, IPescadorService pescadorService)
    {
        _embarcacionService = embarcacionService;
        _pescadorService = pescadorService;
    }

    public List<Embarcacion> Embarcaciones { get; set; } = new();
    public List<Pescador> Pescadores { get; set; } = new();
    
    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Embarcaciones = await _embarcacionService.GetAllAsync();
        Pescadores = await _pescadorService.GetAllAsync();
    }
}
