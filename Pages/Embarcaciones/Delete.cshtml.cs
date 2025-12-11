using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Embarcaciones;

public class DeleteModel : PageModel
{
    private readonly IEmbarcacionService _embarcacionService;
    private readonly IPescadorService _pescadorService;

    public DeleteModel(IEmbarcacionService embarcacionService, IPescadorService pescadorService)
    {
        _embarcacionService = embarcacionService;
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Embarcacion Embarcacion { get; set; } = new();
    public Pescador? Pescador { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var embarcacion = await _embarcacionService.GetByIdAsync(id);
        if (embarcacion == null)
        {
            return NotFound();
        }

        Embarcacion = embarcacion;
        
        if (!string.IsNullOrEmpty(embarcacion.pescadorId))
        {
            Pescador = await _pescadorService.GetByIdAsync(embarcacion.pescadorId);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Embarcacion.Id))
        {
            return NotFound();
        }

        try
        {
            var deleted = await _embarcacionService.DeleteAsync(Embarcacion.Id);
            
            if (!deleted)
            {
                ModelState.AddModelError(string.Empty, "No se pudo eliminar la embarcación");
                return Page();
            }
            
            TempData["SuccessMessage"] = "Embarcación eliminada exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al eliminar embarcación: {ex.Message}");
            
            var embarcacion = await _embarcacionService.GetByIdAsync(Embarcacion.Id);
            if (embarcacion != null)
            {
                Embarcacion = embarcacion;
                if (!string.IsNullOrEmpty(embarcacion.pescadorId))
                {
                    Pescador = await _pescadorService.GetByIdAsync(embarcacion.pescadorId);
                }
            }
            
            return Page();
        }
    }
}
