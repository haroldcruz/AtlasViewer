using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.SitiosPesca;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ISitioPescaService _sitioPescaService;

    public DetailsModel(ISitioPescaService sitioPescaService)
    {
        _sitioPescaService = sitioPescaService;
    }

    public SitioPesca SitioPesca { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var sitioPesca = await _sitioPescaService.GetByIdAsync(id);
        if (sitioPesca == null)
        {
            AlertService.Error(TempData, "Sitio de pesca no encontrado.");
            return RedirectToPage("Index");
        }

        SitioPesca = sitioPesca;
        return Page();
    }
}
