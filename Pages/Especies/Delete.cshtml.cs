using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Especies;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly IEspecieService _especieService;

    public DeleteModel(IEspecieService especieService)
    {
        _especieService = especieService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Especie.Id))
        {
            return NotFound();
        }

        var deleted = await _especieService.DeleteAsync(Especie.Id);
        if (deleted)
        {
            AlertService.Success(TempData, $"Especie '{Especie.nombre}' eliminada exitosamente.");
        }
        else
        {
            AlertService.Error(TempData, "No se pudo eliminar la especie.");
        }

        return RedirectToPage("Index");
    }
}
