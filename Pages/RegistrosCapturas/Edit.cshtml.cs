using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AtlasViewer.Pages.RegistrosCapturas
{
    public class EditModel : PageModel
    {
        private readonly IRegistroCapturaService _registroCapturaService;
        private readonly IPescadorService _pescadorService;
        private readonly IEmbarcacionService _embarcacionService;
        private readonly ISitioPescaService _sitioPescaService;
        private readonly IArtePescaService _artePescaService;
        private readonly IEspecieService _especieService;
        private readonly IInsumoService _insumoService;

        public EditModel(
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

        [BindProperty]
        public string CapturaJson { get; set; } = string.Empty;

        public List<Pescador> Pescadores { get; set; } = new List<Pescador>();
        public List<Embarcacion> Embarcaciones { get; set; } = new List<Embarcacion>();
        public List<SitioPesca> SitiosPesca { get; set; } = new List<SitioPesca>();
        public List<Models.ArtePesca> ArtesPesca { get; set; } = new List<Models.ArtePesca>();
        public List<Especie> Especies { get; set; } = new List<Especie>();
        public List<Insumo> Insumos { get; set; } = new List<Insumo>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            RegistroCaptura = await _registroCapturaService.GetByIdAsync(id);
            
            if (RegistroCaptura == null)
            {
                return NotFound();
            }

            await CargarCatalogosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || RegistroCaptura == null)
            {
                await CargarCatalogosAsync();
                return Page();
            }

            try
            {
                // Parsear JSON de capturas
                if (!string.IsNullOrEmpty(CapturaJson))
                {
                    var capturas = JsonSerializer.Deserialize<List<CapturaDetalle>>(CapturaJson);
                    if (capturas != null && capturas.Count > 0)
                    {
                        RegistroCaptura.capturas = capturas;
                    }
                }

                if (RegistroCaptura.capturas == null || RegistroCaptura.capturas.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Debe agregar al menos una línea de captura");
                    await CargarCatalogosAsync();
                    return Page();
                }

                // Obtener registro anterior para revertir inventarios
                var registroAnterior = await _registroCapturaService.GetByIdAsync(RegistroCaptura.Id!);
                
                // Revertir inventarios del registro anterior
                if (registroAnterior?.capturas != null)
                {
                    foreach (var capturaAnterior in registroAnterior.capturas)
                    {
                        // Revertir especies
                        if (!string.IsNullOrEmpty(capturaAnterior.especieId))
                        {
                            var especie = await _especieService.GetByIdAsync(capturaAnterior.especieId);
                            if (especie != null)
                            {
                                especie.inventarioAcumuladoKg -= capturaAnterior.pesoTotalKg;
                                if (especie.inventarioAcumuladoKg < 0) especie.inventarioAcumuladoKg = 0;
                                await _especieService.UpdateAsync(especie.Id!, especie);
                            }
                        }

                        // Revertir insumos
                        if (capturaAnterior.insumos != null)
                        {
                            foreach (var insumoAnterior in capturaAnterior.insumos)
                            {
                                if (!string.IsNullOrEmpty(insumoAnterior.insumoId))
                                {
                                    var insumo = await _insumoService.GetByIdAsync(insumoAnterior.insumoId);
                                    if (insumo != null)
                                    {
                                        insumo.inventarioActual += insumoAnterior.cantidad;
                                        await _insumoService.UpdateAsync(insumo.Id!, insumo);
                                    }
                                }
                            }
                        }
                    }
                }

                // Actualizar el registro
                await _registroCapturaService.UpdateAsync(RegistroCaptura.Id!, RegistroCaptura);

                // Aplicar nuevos inventarios
                var alertasInventario = new List<string>();

                foreach (var captura in RegistroCaptura.capturas)
                {
                    // Actualizar especies
                    if (!string.IsNullOrEmpty(captura.especieId))
                    {
                        var especie = await _especieService.GetByIdAsync(captura.especieId);
                        if (especie != null)
                        {
                            var inventarioAnterior = especie.inventarioAcumuladoKg;
                            var costoTotalAnterior = inventarioAnterior * (double)especie.precioPromedioCompra;
                            var costoTotalNuevo = captura.pesoTotalKg * (double)captura.precioKilo;
                            var inventarioNuevo = inventarioAnterior + captura.pesoTotalKg;

                            if (inventarioNuevo > 0)
                            {
                                especie.precioPromedioCompra = (decimal)((costoTotalAnterior + costoTotalNuevo) / inventarioNuevo);
                            }
                            else
                            {
                                especie.precioPromedioCompra = captura.precioKilo;
                            }

                            especie.inventarioAcumuladoKg = inventarioNuevo;
                            await _especieService.UpdateAsync(especie.Id!, especie);
                        }
                    }

                    // Descontar insumos
                    if (captura.insumos != null)
                    {
                        foreach (var insumoConsumido in captura.insumos)
                        {
                            if (!string.IsNullOrEmpty(insumoConsumido.insumoId))
                            {
                                var insumo = await _insumoService.GetByIdAsync(insumoConsumido.insumoId);
                                if (insumo != null)
                                {
                                    insumo.inventarioActual -= insumoConsumido.cantidad;

                                    if (insumo.inventarioActual <= 2 && insumo.inventarioActual > 0)
                                    {
                                        alertasInventario.Add($"⚠️ {insumo.nombre}: quedan {insumo.inventarioActual:N2} {insumo.unidad}");
                                    }
                                    else if (insumo.inventarioActual <= 0)
                                    {
                                        alertasInventario.Add($"❌ {insumo.nombre}: AGOTADO");
                                    }

                                    await _insumoService.UpdateAsync(insumo.Id!, insumo);
                                }
                            }
                        }
                    }
                }

                var mensaje = "✅ Registro de captura actualizado exitosamente";
                if (alertasInventario.Any())
                {
                    mensaje += " | INVENTARIO BAJO: " + string.Join(", ", alertasInventario);
                    AlertService.Warning(TempData, mensaje);
                }
                else
                {
                    AlertService.Success(TempData, mensaje);
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al actualizar: {ex.Message}");
                await CargarCatalogosAsync();
                return Page();
            }
        }

        private async Task CargarCatalogosAsync()
        {
            Pescadores = await _pescadorService.GetAllAsync();
            Pescadores = Pescadores.Where(p => p.activo).OrderBy(p => p.nombre).ToList();

            Embarcaciones = await _embarcacionService.GetAllAsync();
            Embarcaciones = Embarcaciones.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            SitiosPesca = await _sitioPescaService.GetAllAsync();
            SitiosPesca = SitiosPesca.Where(s => s.activo).OrderBy(s => s.nombre).ToList();

            ArtesPesca = await _artePescaService.GetAllAsync();
            ArtesPesca = ArtesPesca.Where(a => a.activo).OrderBy(a => a.nombre).ToList();

            Especies = await _especieService.GetAllAsync();
            Especies = Especies.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            Insumos = await _insumoService.GetAllAsync();
            Insumos = Insumos.Where(i => i.activo).OrderBy(i => i.nombre).ToList();
        }
    }
}
