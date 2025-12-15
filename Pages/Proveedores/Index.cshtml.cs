using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.Proveedores;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProveedorService _proveedorService;

    public IndexModel(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    public List<Proveedor> Proveedores { get; set; } = new();

    public async Task OnGetAsync()
    {
        Proveedores = await _proveedorService.GetAllAsync();
    }
}
