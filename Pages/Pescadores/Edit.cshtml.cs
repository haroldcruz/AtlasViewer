using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AtlasViewer.Pages.Pescadores;

public class EditModel : PageModel
{
    private readonly IPescadorService _pescadorService;

    public EditModel(IPescadorService pescadorService)
    {
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Pescador Pescador { get; set; } = new();
    
    [BindProperty]
    public string EmbarcacionesJson { get; set; } = "[]";

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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Parsear embarcaciones desde JSON
            var embarcaciones = JsonSerializer.Deserialize<List<string>>(EmbarcacionesJson);
            if (embarcaciones != null)
            {
                Pescador.embarcaciones = embarcaciones;
            }

            await _pescadorService.UpdateAsync(Pescador.Id!, Pescador);
            
            TempData["SuccessMessage"] = "Pescador actualizado exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar pescador: {ex.Message}");
            return Page();
        }
    }
}
