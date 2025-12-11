using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Pescadores;

public class DeleteModel : PageModel
{
    private readonly IPescadorService _pescadorService;

    public DeleteModel(IPescadorService pescadorService)
    {
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Pescador Pescador { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var pescador = await _pescadorService.GetByIdAsync(id);
        if (pescador == null)
        {
            return NotFound();
        }

        Pescador = pescador;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Pescador.Id))
        {
            return NotFound();
        }

        try
        {
            var deleted = await _pescadorService.DeleteAsync(Pescador.Id);
            
            if (!deleted)
            {
                ModelState.AddModelError(string.Empty, "No se pudo eliminar el pescador");
                return Page();
            }
            
            TempData["SuccessMessage"] = "Pescador eliminado exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al eliminar pescador: {ex.Message}");
            
            // Recargar datos del pescador
            var pescador = await _pescadorService.GetByIdAsync(Pescador.Id);
            if (pescador != null)
            {
                Pescador = pescador;
            }
            
            return Page();
        }
    }
}
