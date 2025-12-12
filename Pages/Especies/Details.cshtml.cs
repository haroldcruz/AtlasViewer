using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Especies;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IEspecieService _especieService;

    public DetailsModel(IEspecieService especieService)
    {
        _especieService = especieService;
    }

    public Especie Especie { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var especie = await _especieService.GetByIdAsync(id);
        if (especie == null)
        {
            AlertService.Error(TempData, "Especie no encontrada.");
            return RedirectToPage("Index");
        }

        Especie = especie;
        return Page();
    }
}
