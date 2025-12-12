using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Especies;

[Authorize(Roles = "Administrador,Editor")]
public class CreateModel : PageModel
{
    private readonly IEspecieService _especieService;

    public CreateModel(IEspecieService especieService)
    {
        _especieService = especieService;
    }

    [BindProperty]
    public Especie Especie { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe
        var existente = await _especieService.GetByCodigoAsync(Especie.codigo);
        if (existente != null)
        {
            AlertService.Error(TempData, $"Ya existe una especie con el código '{Especie.codigo}'.");
            return Page();
        }

        await _especieService.CreateAsync(Especie);
        AlertService.Success(TempData, $"Especie '{Especie.nombre}' creada exitosamente.");
        return RedirectToPage("Index");
    }
}
