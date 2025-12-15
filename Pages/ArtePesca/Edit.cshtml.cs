using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.ArtePesca;

[Authorize(Roles = "Administrador,Editor")]
public class EditModel : PageModel
{
    private readonly IArtePescaService _artePescaService;

    public EditModel(IArtePescaService artePescaService)
    {
        _artePescaService = artePescaService;
    }

    [BindProperty]
    public Models.ArtePesca ArtePesca { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var artePesca = await _artePescaService.GetByIdAsync(id);
        if (artePesca == null)
        {
            AlertService.Error(TempData, "Arte de pesca no encontrado.");
            return RedirectToPage("Index");
        }

        ArtePesca = artePesca;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe en otro arte de pesca
        var existente = await _artePescaService.GetByCodigoAsync(ArtePesca.codigo);
        if (existente != null && existente.Id != ArtePesca.Id)
        {
            AlertService.Error(TempData, $"Ya existe otro arte de pesca con el código '{ArtePesca.codigo}'.");
            return Page();
        }

        await _artePescaService.UpdateAsync(ArtePesca.Id!, ArtePesca);
        AlertService.Success(TempData, $"Arte de pesca '{ArtePesca.nombre}' actualizado exitosamente.");
        return RedirectToPage("Index");
    }
}
