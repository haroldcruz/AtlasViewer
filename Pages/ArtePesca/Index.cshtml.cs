using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.ArtePesca;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IArtePescaService _artePescaService;

    public IndexModel(IArtePescaService artePescaService)
    {
        _artePescaService = artePescaService;
    }

    public List<Models.ArtePesca> ArtesPesca { get; set; } = new();

    public async Task OnGetAsync()
    {
        ArtesPesca = await _artePescaService.GetAllAsync();
    }
}
