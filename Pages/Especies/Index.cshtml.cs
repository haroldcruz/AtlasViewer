using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Especies;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IEspecieService _especieService;

    public IndexModel(IEspecieService especieService)
    {
        _especieService = especieService;
    }

    public List<Especie> Especies { get; set; } = new();

    public async Task OnGetAsync()
    {
        Especies = await _especieService.GetAllAsync();
    }
}
