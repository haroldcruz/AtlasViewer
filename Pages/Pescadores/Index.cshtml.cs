using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Pescadores;

public class IndexModel : PageModel
{
    private readonly IPescadorService _pescadorService;

    public IndexModel(IPescadorService pescadorService)
    {
        _pescadorService = pescadorService;
    }

    public List<Pescador> Pescadores { get; set; } = new();
    
    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Pescadores = await _pescadorService.GetAllAsync();
    }
}
