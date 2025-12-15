using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.ArtePesca;

[Authorize(Roles = "Administrador,Editor")]
public class CreateModel : PageModel
{
    private readonly IArtePescaService _artePescaService;

    public CreateModel(IArtePescaService artePescaService)
    {
        _artePescaService = artePescaService;
    }

    [BindProperty]
    public Models.ArtePesca ArtePesca { get; set; } = new();

    public void OnGet()
    {
        // Por defecto, el arte de pesca está activo
        ArtePesca.activo = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe
        var existente = await _artePescaService.GetByCodigoAsync(ArtePesca.codigo);
        if (existente != null)
        {
            AlertService.Error(TempData, $"Ya existe un arte de pesca con el código '{ArtePesca.codigo}'.");
            return Page();
        }

        await _artePescaService.CreateAsync(ArtePesca);
        AlertService.Success(TempData, $"Arte de pesca '{ArtePesca.nombre}' creado exitosamente.");
        return RedirectToPage("Index");
    }
}
