using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.SitiosPesca;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ISitioPescaService _sitioPescaService;

    public IndexModel(ISitioPescaService sitioPescaService)
    {
        _sitioPescaService = sitioPescaService;
    }

    public List<SitioPesca> SitiosPesca { get; set; } = new();

    public async Task OnGetAsync()
    {
        SitiosPesca = await _sitioPescaService.GetAllAsync();
    }
}
