using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AtlasViewer.Pages.Pescadores;

public class CreateModel : PageModel
{
    private readonly IPescadorService _pescadorService;

    public CreateModel(IPescadorService pescadorService)
    {
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Pescador Pescador { get; set; } = new();
    
    [BindProperty]
    public string EmbarcacionesJson { get; set; } = "[]";

    public void OnGet()
    {
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

            await _pescadorService.CreateAsync(Pescador);
            
            TempData["SuccessMessage"] = "Pescador creado exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al crear pescador: {ex.Message}");
            return Page();
        }
    }
}
