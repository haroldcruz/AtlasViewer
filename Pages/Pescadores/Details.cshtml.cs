using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Pescadores;

public class DetailsModel : PageModel
{
    private readonly IPescadorService _pescadorService;

    public DetailsModel(IPescadorService pescadorService)
    {
        _pescadorService = pescadorService;
    }

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
}
