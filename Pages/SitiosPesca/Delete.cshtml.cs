using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.SitiosPesca;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly ISitioPescaService _sitioPescaService;

    public DeleteModel(ISitioPescaService sitioPescaService)
    {
        _sitioPescaService = sitioPescaService;
    }

    [BindProperty]
    public SitioPesca SitioPesca { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var sitioPesca = await _sitioPescaService.GetByIdAsync(id);
        if (sitioPesca == null)
        {
            AlertService.Error(TempData, "Sitio de pesca no encontrado.");
            return RedirectToPage("Index");
        }

        SitioPesca = sitioPesca;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(SitioPesca.Id))
        {
            return NotFound();
        }

        var deleted = await _sitioPescaService.DeleteAsync(SitioPesca.Id);
        if (deleted)
        {
            AlertService.Success(TempData, $"Sitio de pesca '{SitioPesca.nombre}' eliminado exitosamente.");
        }
        else
        {
            AlertService.Error(TempData, "No se pudo eliminar el sitio de pesca.");
        }

        return RedirectToPage("Index");
    }
}
