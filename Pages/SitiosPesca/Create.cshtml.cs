using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.SitiosPesca;

[Authorize(Roles = "Administrador,Editor")]
public class CreateModel : PageModel
{
    private readonly ISitioPescaService _sitioPescaService;

    public CreateModel(ISitioPescaService sitioPescaService)
    {
        _sitioPescaService = sitioPescaService;
    }

    [BindProperty]
    public SitioPesca SitioPesca { get; set; } = new();

    public void OnGet()
    {
        // Por defecto, el sitio está activo
        SitioPesca.activo = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe
        var existente = await _sitioPescaService.GetByCodigoAsync(SitioPesca.codigo);
        if (existente != null)
        {
            AlertService.Error(TempData, $"Ya existe un sitio de pesca con el código '{SitioPesca.codigo}'.");
            return Page();
        }

        await _sitioPescaService.CreateAsync(SitioPesca);
        AlertService.Success(TempData, $"Sitio de pesca '{SitioPesca.nombre}' creado exitosamente.");
        return RedirectToPage("Index");
    }
}
