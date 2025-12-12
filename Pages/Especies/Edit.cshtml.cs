using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Especies;

[Authorize(Roles = "Administrador,Editor")]
public class EditModel : PageModel
{
    private readonly IEspecieService _especieService;

    public EditModel(IEspecieService especieService)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe en otra especie
        var existente = await _especieService.GetByCodigoAsync(Especie.codigo);
        if (existente != null && existente.Id != Especie.Id)
        {
            AlertService.Error(TempData, $"Ya existe otra especie con el código '{Especie.codigo}'.");
            return Page();
        }

        await _especieService.UpdateAsync(Especie.Id!, Especie);
        AlertService.Success(TempData, $"Especie '{Especie.nombre}' actualizada exitosamente.");
        return RedirectToPage("Index");
    }
}
