using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Insumos;

[Authorize]
public class EditModel : PageModel
{
    private readonly IInsumoService _insumoService;

    public EditModel(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Insumo == null)
        {
            return Page();
        }

        // Verificar código duplicado (excepto el mismo registro)
        var existente = await _insumoService.GetByCodigoAsync(Insumo.codigo);
        if (existente != null && existente.Id != Insumo.Id)
        {
            AlertService.Error(TempData, $"Ya existe otro insumo con el código '{Insumo.codigo}'.");
            return Page();
        }

        await _insumoService.UpdateAsync(Insumo.Id!, Insumo);
        AlertService.Success(TempData, $"Insumo '{Insumo.nombre}' actualizado exitosamente.");
        return RedirectToPage("Index");
    }
}
