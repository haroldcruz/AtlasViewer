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
        private readonly IMonitoreoBiologicoService _monitoreoService;
        private readonly IPescaIncidentalService _incidentalService;
        private readonly IPescaFantasmaService _fantasmaService;

        public EditModel(
            IRegistroCapturaService registroCapturaService,
            IPescadorService pescadorService,
            IEmbarcacionService embarcacionService,
            ISitioPescaService sitioPescaService,
            IArtePescaService artePescaService,
            IEspecieService especieService,
            IInsumoService insumoService,
            IMonitoreoBiologicoService monitoreoService,
            IPescaIncidentalService incidentalService,
            IPescaFantasmaService fantasmaService)
        {
            _registroCapturaService = registroCapturaService;
            _pescadorService = pescadorService;
            _embarcacionService = embarcacionService;
            _sitioPescaService = sitioPescaService;
            _artePescaService = artePescaService;
            _especieService = especieService;
            _insumoService = insumoService;
            _monitoreoService = monitoreoService;
            _incidentalService = incidentalService;
            _fantasmaService = fantasmaService;
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
        public List<MonitoreoBiologico> MonitoreosExistentes { get; set; } = new List<MonitoreoBiologico>();
        public string MonitoreosJson { get; set; } = string.Empty;
        public List<PescaIncidental> IncidentalesExistentes { get; set; } = new List<PescaIncidental>();
        public string IncidentalesJson { get; set; } = string.Empty;
        public List<PescaFantasma> FantasmasExistentes { get; set; } = new List<PescaFantasma>();
        public string FantasmasJson { get; set; } = string.Empty;

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

            // Cargar monitoreos biológicos existentes
            MonitoreosExistentes = await _monitoreoService.GetByRegistroAsync(id);
            if (MonitoreosExistentes.Any())
            {
                MonitoreosJson = JsonSerializer.Serialize(MonitoreosExistentes);
            }

            // Cargar pesca incidental existente
            IncidentalesExistentes = await _incidentalService.GetByRegistroAsync(id);
            if (IncidentalesExistentes.Any())
            {
                IncidentalesJson = JsonSerializer.Serialize(IncidentalesExistentes);
            }

            // Cargar pesca fantasma existente
            FantasmasExistentes = await _fantasmaService.GetByRegistroAsync(id);
            if (FantasmasExistentes.Any())
            {
                FantasmasJson = JsonSerializer.Serialize(FantasmasExistentes);
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

                // Eliminar monitoreos anteriores
                var monitoreosAnteriores = await _monitoreoService.GetByRegistroAsync(RegistroCaptura.Id!);
                foreach (var monitoreoAnterior in monitoreosAnteriores)
                {
                    await _monitoreoService.DeleteAsync(monitoreoAnterior.Id!);
                }

                // Procesar Monitoreo Biológico (opcional)
                var monitoreoKeys = Request.Form.Keys.Where(k => k.StartsWith("Monitoreo[") && k.EndsWith("].especieId")).ToList();
                foreach (var key in monitoreoKeys)
                {
                    var indexStr = key.Replace("Monitoreo[", "").Replace("].especieId", "");
                    var especieId = Request.Form[$"Monitoreo[{indexStr}].especieId"].ToString();
                    
                    if (!string.IsNullOrEmpty(especieId))
                    {
                        var tallaCmStr = Request.Form[$"Monitoreo[{indexStr}].tallaCm"].ToString();
                        var pesoKgStr = Request.Form[$"Monitoreo[{indexStr}].pesoKg"].ToString();
                        
                        // Solo guardar si tiene al menos talla
                        if (!string.IsNullOrWhiteSpace(tallaCmStr))
                        {
                            var monitoreo = new MonitoreoBiologico
                            {
                                registroId = RegistroCaptura.Id,
                                especieId = especieId,
                                tallaCm = double.Parse(tallaCmStr),
                                pesoKg = string.IsNullOrWhiteSpace(pesoKgStr) ? 0 : double.Parse(pesoKgStr),
                                sexo = Request.Form[$"Monitoreo[{indexStr}].sexo"].ToString() ?? "",
                                madurez = Request.Form[$"Monitoreo[{indexStr}].madurez"].ToString() ?? "",
                                eviscerado = Request.Form[$"Monitoreo[{indexStr}].eviscerado"].ToString() == "true",
                                observaciones = Request.Form[$"Monitoreo[{indexStr}].observaciones"].ToString() ?? ""
                            };
                            await _monitoreoService.CreateAsync(monitoreo);
                        }
                    }
                }

                // Eliminar pesca incidental anterior
                var incidentalesAnteriores = await _incidentalService.GetByRegistroAsync(RegistroCaptura.Id!);
                foreach (var incidentalAnterior in incidentalesAnteriores)
                {
                    await _incidentalService.DeleteAsync(incidentalAnterior.Id!);
                }

                // Procesar Pesca Incidental (opcional)
                var incidentalKeys = Request.Form.Keys.Where(k => k.StartsWith("Incidental[") && k.EndsWith("].especieId")).ToList();
                foreach (var key in incidentalKeys)
                {
                    var indexStr = key.Replace("Incidental[", "").Replace("].especieId", "");
                    var especieId = Request.Form[$"Incidental[{indexStr}].especieId"].ToString();
                    
                    if (!string.IsNullOrEmpty(especieId))
                    {
                        var individuosStr = Request.Form[$"Incidental[{indexStr}].individuos"].ToString();
                        var pesoKgStr = Request.Form[$"Incidental[{indexStr}].pesoTotalKg"].ToString();
                        
                        // Solo guardar si tiene individuos o peso
                        if (!string.IsNullOrWhiteSpace(individuosStr) || !string.IsNullOrWhiteSpace(pesoKgStr))
                        {
                            var incidental = new PescaIncidental
                            {
                                registroId = RegistroCaptura.Id,
                                especieId = especieId,
                                individuos = string.IsNullOrWhiteSpace(individuosStr) ? 0 : int.Parse(individuosStr),
                                pesoTotalKg = string.IsNullOrWhiteSpace(pesoKgStr) ? 0 : double.Parse(pesoKgStr)
                            };
                            await _incidentalService.CreateAsync(incidental);
                        }
                    }
                }

                // Eliminar pesca fantasma anterior
                var fantasmasAnteriores = await _fantasmaService.GetByRegistroAsync(RegistroCaptura.Id!);
                foreach (var fantasmaAnterior in fantasmasAnteriores)
                {
                    await _fantasmaService.DeleteAsync(fantasmaAnterior.Id!);
                }

                // Procesar Pesca Fantasma (opcional)
                var fantasmaKeys = Request.Form.Keys.Where(k => k.StartsWith("Fantasma[") && k.EndsWith("].tipoArte")).ToList();
                foreach (var key in fantasmaKeys)
                {
                    var indexStr = key.Replace("Fantasma[", "").Replace("].tipoArte", "");
                    var tipoArte = Request.Form[$"Fantasma[{indexStr}].tipoArte"].ToString();
                    
                    if (!string.IsNullOrWhiteSpace(tipoArte))
                    {
                        var fantasma = new PescaFantasma
                        {
                            registroId = RegistroCaptura.Id,
                            tipoArte = tipoArte,
                            especiesAfectadas = Request.Form[$"Fantasma[{indexStr}].especiesAfectadas"].ToString() ?? "",
                            ubicacion = Request.Form[$"Fantasma[{indexStr}].ubicacion"].ToString() ?? "",
                            liberacion = Request.Form[$"Fantasma[{indexStr}].liberacion"].ToString() == "true"
                        };
                        await _fantasmaService.CreateAsync(fantasma);
                    }
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
