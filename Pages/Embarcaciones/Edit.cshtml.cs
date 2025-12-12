using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Embarcaciones;

public class EditModel : PageModel
{
    private readonly IEmbarcacionService _embarcacionService;
    private readonly IPescadorService _pescadorService;

    public EditModel(IEmbarcacionService embarcacionService, IPescadorService pescadorService)
    {
        _embarcacionService = embarcacionService;
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Embarcacion Embarcacion { get; set; } = new();
    
    public List<Pescador> Pescadores { get; set; } = new();

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
        Pescadores = await _pescadorService.GetAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Normalizar pescadorId antes de validar
        if (string.IsNullOrWhiteSpace(Embarcacion.pescadorId) || Embarcacion.pescadorId == "undefined")
        {
            Embarcacion.pescadorId = null;
        }
        
        // Validar que pescadorId no sea nulo
        if (string.IsNullOrEmpty(Embarcacion.pescadorId))
        {
            ModelState.AddModelError("Embarcacion.pescadorId", "Debe seleccionar un pescador (propietario).");
        }
        
        if (!ModelState.IsValid)
        {
            Pescadores = await _pescadorService.GetAllAsync();
            return Page();
        }

        try
        {
            await _embarcacionService.UpdateAsync(Embarcacion.Id!, Embarcacion);
            
            TempData["SuccessMessage"] = "Embarcación actualizada exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar embarcación: {ex.Message}");
            Pescadores = await _pescadorService.GetAllAsync();
            return Page();
        }
    }
}
