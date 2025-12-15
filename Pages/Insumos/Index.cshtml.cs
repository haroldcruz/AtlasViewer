using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Insumos;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IInsumoService _insumoService;

    public IndexModel(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    public List<Insumo> Insumos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Insumos = await _insumoService.GetAllAsync();
    }
}
