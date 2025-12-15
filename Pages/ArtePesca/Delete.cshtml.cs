using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.ArtePesca;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly IArtePescaService _artePescaService;

    public DeleteModel(IArtePescaService artePescaService)
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
        if (string.IsNullOrEmpty(ArtePesca.Id))
        {
            return NotFound();
        }

        var deleted = await _artePescaService.DeleteAsync(ArtePesca.Id);
        if (deleted)
        {
            AlertService.Success(TempData, $"Arte de pesca '{ArtePesca.nombre}' eliminado exitosamente.");
        }
        else
        {
            AlertService.Error(TempData, "No se pudo eliminar el arte de pesca.");
        }

        return RedirectToPage("Index");
    }
}
