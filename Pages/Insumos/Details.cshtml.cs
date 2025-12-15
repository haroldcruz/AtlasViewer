using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Insumos;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IInsumoService _insumoService;

    public DetailsModel(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    public Insumo? Insumo { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        Insumo = await _insumoService.GetByIdAsync(id);

        if (Insumo == null)
        {
            AlertService.Error(TempData, "Insumo no encontrado.");
            return RedirectToPage("Index");
        }

        return Page();
    }
}
