using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.RegistrosCapturas
{
    public class DeleteModel : PageModel
    {
        private readonly IRegistroCapturaService _registroCapturaService;
        private readonly IPescadorService _pescadorService;
        private readonly IEmbarcacionService _embarcacionService;
        private readonly ISitioPescaService _sitioPescaService;
        private readonly IArtePescaService _artePescaService;
        private readonly IEspecieService _especieService;
        private readonly IInsumoService _insumoService;

        public DeleteModel(
            IRegistroCapturaService registroCapturaService,
            IPescadorService pescadorService,
            IEmbarcacionService embarcacionService,
            ISitioPescaService sitioPescaService,
            IArtePescaService artePescaService,
            IEspecieService especieService,
            IInsumoService insumoService)
        {
            _registroCapturaService = registroCapturaService;
            _pescadorService = pescadorService;
            _embarcacionService = embarcacionService;
            _sitioPescaService = sitioPescaService;
            _artePescaService = artePescaService;
            _especieService = especieService;
            _insumoService = insumoService;
        }

        [BindProperty]
        public RegistroCaptura? RegistroCaptura { get; set; }
        public string PescadorNombre { get; set; } = string.Empty;
        public string EmbarcacionNombre { get; set; } = string.Empty;
        public string SitioNombre { get; set; } = string.Empty;
        public string ArteNombre { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            RegistroCaptura = await _registroCapturaService.GetByIdAsync(id);

            if (RegistroCaptura == null)
            {
                return Page();
            }

            // Cargar nombres de referencias
            var pescador = await _pescadorService.GetByIdAsync(RegistroCaptura.pescadorId ?? "");
            PescadorNombre = pescador?.nombre ?? "No encontrado";

            var embarcacion = await _embarcacionService.GetByIdAsync(RegistroCaptura.embarcacionId ?? "");
            EmbarcacionNombre = embarcacion?.matricula ?? embarcacion?.nombre ?? "No encontrado";

            if (!string.IsNullOrEmpty(RegistroCaptura.sitioPescaId))
            {
                var sitio = await _sitioPescaService.GetByIdAsync(RegistroCaptura.sitioPescaId);
                SitioNombre = sitio?.nombre ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(RegistroCaptura.artePescaId))
            {
                var arte = await _artePescaService.GetByIdAsync(RegistroCaptura.artePescaId);
                ArteNombre = arte?.nombre ?? string.Empty;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (RegistroCaptura == null || string.IsNullOrEmpty(RegistroCaptura.Id))
            {
                return NotFound();
            }

            try
            {
                // Recuperar el registro completo antes de eliminar
                var registroAnterior = await _registroCapturaService.GetByIdAsync(RegistroCaptura.Id);
                if (registroAnterior == null)
                {
                    AlertService.Error(TempData, "No se encontró el registro a eliminar");
                    return RedirectToPage("Index");
                }

                // Revertir inventarios de especies
                if (registroAnterior.capturas != null && registroAnterior.capturas.Any())
                {
                    foreach (var captura in registroAnterior.capturas)
                    {
                        var especie = await _especieService.GetByIdAsync(captura.especieId ?? "");
                        if (especie != null)
                        {
                            especie.inventarioAcumuladoKg -= captura.pesoTotalKg;
                            
                            // Si el inventario queda en 0 o negativo, resetear precio promedio
                            if (especie.inventarioAcumuladoKg <= 0)
                            {
                                especie.inventarioAcumuladoKg = 0;
                                especie.precioPromedioCompra = 0;
                            }
                            else
                            {
                                // Recalcular precio promedio sin esta captura
                                // Como no tenemos el historial, mantener el precio actual
                                // En un sistema más robusto, se guardaría el historial de precios
                            }
                            
                            await _especieService.UpdateAsync(especie.Id!, especie);
                        }
                    }
                }

                // Revertir inventarios de insumos
                if (registroAnterior.capturas != null)
                {
                    foreach (var captura in registroAnterior.capturas)
                    {
                        if (captura.insumos != null && captura.insumos.Any())
                        {
                            foreach (var insumoConsumido in captura.insumos)
                            {
                                if (!string.IsNullOrEmpty(insumoConsumido.insumoId))
                                {
                                    var insumo = await _insumoService.GetByIdAsync(insumoConsumido.insumoId);
                                    if (insumo != null)
                                    {
                                        // Devolver al inventario la cantidad que se había consumido
                                        insumo.inventarioActual += insumoConsumido.cantidad;
                                        await _insumoService.UpdateAsync(insumo.Id!, insumo);
                                    }
                                }
                            }
                        }
                    }
                }

                // Eliminar el registro
                await _registroCapturaService.DeleteAsync(RegistroCaptura.Id);
                AlertService.Success(TempData, "Registro de captura eliminado e inventarios revertidos exitosamente");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                AlertService.Error(TempData, $"Error al eliminar el registro: {ex.Message}");
                return RedirectToPage("Index");
            }
        }
    }
}
