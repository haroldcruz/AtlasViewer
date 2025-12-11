using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Embarcaciones;

public class CreateModel : PageModel
{
    private readonly IEmbarcacionService _embarcacionService;
    private readonly IPescadorService _pescadorService;

    public CreateModel(IEmbarcacionService embarcacionService, IPescadorService pescadorService)
    {
        _embarcacionService = embarcacionService;
        _pescadorService = pescadorService;
    }

    [BindProperty]
    public Embarcacion Embarcacion { get; set; } = new();
    
    public List<Pescador> Pescadores { get; set; } = new();

    public async Task OnGetAsync()
    {
        Pescadores = await _pescadorService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Pescadores = await _pescadorService.GetAllAsync();
            return Page();
        }

        try
        {
            await _embarcacionService.CreateAsync(Embarcacion);
            
            TempData["SuccessMessage"] = "Embarcación creada exitosamente";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al crear embarcación: {ex.Message}");
            Pescadores = await _pescadorService.GetAllAsync();
            return Page();
        }
    }
}
