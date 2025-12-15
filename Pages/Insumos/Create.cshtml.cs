using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Insumos;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IInsumoService _insumoService;

    public CreateModel(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    [BindProperty]
    public Insumo Insumo { get; set; } = new Insumo();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar que el código no exista
        var existente = await _insumoService.GetByCodigoAsync(Insumo.codigo);
        if (existente != null)
        {
            AlertService.Error(TempData, $"Ya existe un insumo con el código '{Insumo.codigo}'.");
            return Page();
        }

        await _insumoService.CreateAsync(Insumo);
        AlertService.Success(TempData, $"Insumo '{Insumo.nombre}' creado exitosamente.");
        return RedirectToPage("Index");
    }
}
