using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;

namespace AtlasViewer.Pages.SitiosPesca;

[Authorize(Roles = "Administrador,Editor")]
public class EditModel : PageModel
{
    private readonly ISitioPescaService _sitioPescaService;

    public EditModel(ISitioPescaService sitioPescaService)
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Verificar si el código ya existe en otro sitio
        var existente = await _sitioPescaService.GetByCodigoAsync(SitioPesca.codigo);
        if (existente != null && existente.Id != SitioPesca.Id)
        {
            AlertService.Error(TempData, $"Ya existe otro sitio de pesca con el código '{SitioPesca.codigo}'.");
            return Page();
        }

        await _sitioPescaService.UpdateAsync(SitioPesca.Id!, SitioPesca);
        AlertService.Success(TempData, $"Sitio de pesca '{SitioPesca.nombre}' actualizado exitosamente.");
        return RedirectToPage("Index");
    }
}
