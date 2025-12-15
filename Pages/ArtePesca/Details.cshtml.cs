using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.ArtePesca;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IArtePescaService _artePescaService;

    public DetailsModel(IArtePescaService artePescaService)
    {
        _artePescaService = artePescaService;
    }

    public Models.ArtePesca ArtePesca { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var artePesca = await _artePescaService.GetByIdAsync(id);
        if (artePesca == null)
        {
            AlertService.Error(TempData, "Arte de pesca no encontrado.");
            return RedirectToPage("Index");
        }

        ArtePesca = artePesca;
        return Page();
    }
}
