using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Insumos;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IInsumoService _insumoService;

    public DeleteModel(IInsumoService insumoService)
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
        if (Insumo == null || string.IsNullOrEmpty(Insumo.Id))
        {
            return NotFound();
        }

        var resultado = await _insumoService.DeleteAsync(Insumo.Id);
        
        if (resultado)
        {
            AlertService.Success(TempData, $"Insumo '{Insumo.nombre}' eliminado exitosamente.");
        }
        else
        {
            AlertService.Error(TempData, "No se pudo eliminar el insumo.");
        }

        return RedirectToPage("Index");
    }
}
