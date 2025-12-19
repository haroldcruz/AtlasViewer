using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace AtlasViewer.Pages.RegistrosCapturas
{
    public class CreateModel : PageModel
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

        public CreateModel(
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
        public RegistroCaptura RegistroCaptura { get; set; } = new RegistroCaptura();

        public List<Pescador> Pescadores { get; set; } = new List<Pescador>();
        public List<Embarcacion> Embarcaciones { get; set; } = new List<Embarcacion>();
        public List<SitioPesca> SitiosPesca { get; set; } = new List<SitioPesca>();
        public List<Models.ArtePesca> ArtesPesca { get; set; } = new List<Models.ArtePesca>();
        public List<Especie> Especies { get; set; } = new List<Especie>();
        public List<Insumo> Insumos { get; set; } = new List<Insumo>();
        
        // Propiedades para preservar datos cuando hay error
        [TempData]
        public string? CapturasDatos { get; set; }
        [TempData]
        public string? InsumosDatos { get; set; }

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();
            
            // Establecer fecha y hora actuales
            RegistroCaptura.fecha = DateTime.Now.Date;
            RegistroCaptura.hora = DateTime.Now.ToString("HH:mm");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validar pescador
            if (string.IsNullOrEmpty(RegistroCaptura.pescadorId))
            {
                ModelState.AddModelError("RegistroCaptura.pescadorId", "Debe seleccionar un pescador");
            }

            // Validar embarcación
            if (string.IsNullOrEmpty(RegistroCaptura.embarcacionId))
            {
                ModelState.AddModelError("RegistroCaptura.embarcacionId", "Debe seleccionar una embarcación");
            }

            // Procesar capturas
            RegistroCaptura.capturas = new List<CapturaDetalle>();
            var capturaKeys = Request.Form.Keys.Where(k => k.StartsWith("Capturas[") && k.EndsWith("].especieId")).ToList();
            
            foreach (var key in capturaKeys)
            {
                var indexStr = key.Replace("Capturas[", "").Replace("].especieId", "");
                var especieId = Request.Form[$"Capturas[{indexStr}].especieId"].ToString();
                
                if (!string.IsNullOrEmpty(especieId))
                {
                    var numeroPecesStr = Request.Form[$"Capturas[{indexStr}].numeroPeces"].ToString();
                    var pesoTotalStr = Request.Form[$"Capturas[{indexStr}].pesoTotalKg"].ToString();
                    var precioKiloStr = Request.Form[$"Capturas[{indexStr}].precioKilo"].ToString();
                    var totalStr = Request.Form[$"Capturas[{indexStr}].total"].ToString();
                    
                    var numeroPeces = int.Parse(string.IsNullOrWhiteSpace(numeroPecesStr) ? "0" : numeroPecesStr);
                    var pesoTotal = double.Parse(string.IsNullOrWhiteSpace(pesoTotalStr) ? "0" : pesoTotalStr);
                    var precioKilo = decimal.Parse(string.IsNullOrWhiteSpace(precioKiloStr) ? "0" : precioKiloStr);
                    var total = decimal.Parse(string.IsNullOrWhiteSpace(totalStr) ? "0" : totalStr);
                    
                    // Solo agregar si tiene datos válidos (peso y precio)
                    if (pesoTotal > 0 && precioKilo > 0)
                    {
                        // Verificar duplicados
                        if (RegistroCaptura.capturas.Any(c => c.especieId == especieId))
                        {
                            ModelState.AddModelError("", $"La especie con ID {especieId} ya fue agregada. No se permiten duplicados.");
                            continue;
                        }
                        
                        var captura = new CapturaDetalle
                        {
                            especieId = especieId,
                            numeroPeces = numeroPeces,
                            pesoTotalKg = pesoTotal,
                            precioKilo = precioKilo,
                            total = total,
                            insumos = new List<AlistoInsumo>()
                        };
                        RegistroCaptura.capturas.Add(captura);
                    }
                }
            }

            // Procesar insumos (globales - se pueden asociar a capturas específicas más adelante)
            // Por ahora, los agregaremos a la primera captura si existe
            var insumoKeys = Request.Form.Keys.Where(k => k.StartsWith("Insumos[") && k.EndsWith("].insumoId")).ToList();
            var insumosGlobales = new List<AlistoInsumo>();
            
            foreach (var key in insumoKeys)
            {
                var indexStr = key.Replace("Insumos[", "").Replace("].insumoId", "");
                var insumoId = Request.Form[$"Insumos[{indexStr}].insumoId"].ToString();
                
                if (!string.IsNullOrEmpty(insumoId))
                {
                    var cantidadStr = Request.Form[$"Insumos[{indexStr}].cantidad"].ToString();
                    var precioUnitarioStr = Request.Form[$"Insumos[{indexStr}].precioUnitario"].ToString();
                    var subtotalStr = Request.Form[$"Insumos[{indexStr}].subtotal"].ToString();
                    
                    var cantidad = double.Parse(string.IsNullOrWhiteSpace(cantidadStr) ? "0" : cantidadStr);
                    var precioUnitario = decimal.Parse(string.IsNullOrWhiteSpace(precioUnitarioStr) ? "0" : precioUnitarioStr);
                    var subtotal = decimal.Parse(string.IsNullOrWhiteSpace(subtotalStr) ? "0" : subtotalStr);
                    
                    // Solo agregar si tiene datos válidos (cantidad y precio)
                    if (cantidad > 0 && precioUnitario > 0)
                    {
                        // Verificar duplicados
                        if (insumosGlobales.Any(i => i.insumoId == insumoId))
                        {
                            ModelState.AddModelError("", $"El insumo con ID {insumoId} ya fue agregado. No se permiten duplicados.");
                            continue;
                        }
                        
                        var insumo = new AlistoInsumo
                        {
                            insumoId = insumoId,
                            cantidad = cantidad,
                            precioUnitario = precioUnitario,
                            subtotal = subtotal
                        };
                        insumosGlobales.Add(insumo);
                    }
                }
            }

            // Asociar insumos a la primera captura (lógica temporal)
            if (RegistroCaptura.capturas.Count > 0 && insumosGlobales.Count > 0)
            {
                RegistroCaptura.capturas[0].insumos = insumosGlobales;
            }

            // Validar que haya al menos una captura
            if (RegistroCaptura.capturas.Count == 0)
            {
                // Preservar datos de capturas e insumos para que no se pierdan
                CapturasDatos = System.Text.Json.JsonSerializer.Serialize(Request.Form.Keys
                    .Where(k => k.StartsWith("Capturas["))
                    .Select(k => new { key = k, value = Request.Form[k].ToString() })
                    .ToList());
                    
                InsumosDatos = System.Text.Json.JsonSerializer.Serialize(Request.Form.Keys
                    .Where(k => k.StartsWith("Insumos["))
                    .Select(k => new { key = k, value = Request.Form[k].ToString() })
                    .ToList());
                
                AlertService.Error(TempData, "Debe registrar al menos una captura de especie con peso y precio válidos.");
                await CargarCatalogosAsync();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                // Preservar datos de capturas e insumos
                CapturasDatos = System.Text.Json.JsonSerializer.Serialize(Request.Form.Keys
                    .Where(k => k.StartsWith("Capturas["))
                    .Select(k => new { key = k, value = Request.Form[k].ToString() })
                    .ToList());
                    
                InsumosDatos = System.Text.Json.JsonSerializer.Serialize(Request.Form.Keys
                    .Where(k => k.StartsWith("Insumos["))
                    .Select(k => new { key = k, value = Request.Form[k].ToString() })
                    .ToList());
                
                var errores = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                AlertService.Error(TempData, $"Por favor corrija los siguientes errores: {errores}");
                await CargarCatalogosAsync();
                return Page();
            }

            try
            {
                // Crear el registro
                await _registroCapturaService.CreateAsync(RegistroCaptura);

                // Actualizar inventarios de especies y calcular precios promedio
                var alertasInventario = new List<string>();
                
                foreach (var captura in RegistroCaptura.capturas)
                {
                    if (!string.IsNullOrEmpty(captura.especieId))
                    {
                        var especie = await _especieService.GetByIdAsync(captura.especieId);
                        if (especie != null)
                        {
                            // Calcular precio promedio ponderado de compra
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

                            // Actualizar inventario
                            especie.inventarioAcumuladoKg = inventarioNuevo;

                            await _especieService.UpdateAsync(especie.Id!, especie);
                        }
                    }

                    // Descontar insumos del inventario
                    if (captura.insumos != null)
                    {
                        foreach (var insumoConsumido in captura.insumos)
                        {
                            if (!string.IsNullOrEmpty(insumoConsumido.insumoId))
                            {
                                var insumo = await _insumoService.GetByIdAsync(insumoConsumido.insumoId);
                                if (insumo != null)
                                {
                                    // Descontar del inventario
                                    insumo.inventarioActual -= insumoConsumido.cantidad;

                                    // Verificar inventario bajo
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

                // Mensaje de éxito con alertas de inventario
                var mensaje = "✅ Registro de captura creado exitosamente";
                if (alertasInventario.Any())
                {
                    mensaje += " | INVENTARIO BAJO: " + string.Join(", ", alertasInventario);
                    AlertService.Warning(TempData, mensaje);
                }
                else
                {
                    AlertService.Success(TempData, mensaje);
                }

                // Procesar Monitoreo Biológico (opcional)
                var monitoreoKeys = Request.Form.Keys.Where(k => k.StartsWith("Monitoreo[") && k.EndsWith("].especieId")).ToList();
                Console.WriteLine($"[DEBUG] Encontradas {monitoreoKeys.Count} claves de monitoreo");
                
                foreach (var key in monitoreoKeys)
                {
                    var indexStr = key.Replace("Monitoreo[", "").Replace("].especieId", "");
                    var especieId = Request.Form[$"Monitoreo[{indexStr}].especieId"].ToString();
                    Console.WriteLine($"[DEBUG] Procesando monitoreo índice {indexStr}, especieId: {especieId}");
                    
                    if (!string.IsNullOrEmpty(especieId))
                    {
                        var tallaCmStr = Request.Form[$"Monitoreo[{indexStr}].tallaCm"].ToString();
                        var pesoKgStr = Request.Form[$"Monitoreo[{indexStr}].pesoKg"].ToString();
                        Console.WriteLine($"[DEBUG] Talla: {tallaCmStr}, Peso: {pesoKgStr}");
                        
                        // Solo guardar si tiene al menos talla
                        if (!string.IsNullOrWhiteSpace(tallaCmStr))
                        {
                            var monitoreo = new AtlasViewer.Models.MonitoreoBiologico
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
                            Console.WriteLine($"[DEBUG] Guardando monitoreo: registroId={monitoreo.registroId}, talla={monitoreo.tallaCm}");
                            await _monitoreoService.CreateAsync(monitoreo);
                            Console.WriteLine($"[DEBUG] Monitoreo guardado exitosamente");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Monitoreo {indexStr} omitido - sin talla");
                        }
                    }
                }

                // Procesar Pesca Incidental (opcional)
                var incidentalKeys = Request.Form.Keys.Where(k => k.StartsWith("Incidental[") && k.EndsWith("].especieId")).ToList();
                Console.WriteLine($"[DEBUG] Encontradas {incidentalKeys.Count} claves de pesca incidental");
                
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
                            Console.WriteLine($"[DEBUG] Guardando pesca incidental: especieId={incidental.especieId}, individuos={incidental.individuos}");
                            await _incidentalService.CreateAsync(incidental);
                        }
                    }
                }

                // Procesar Pesca Fantasma (opcional)
                var fantasmaKeys = Request.Form.Keys.Where(k => k.StartsWith("Fantasma[") && k.EndsWith("].tipoArte")).ToList();
                Console.WriteLine($"[DEBUG] Encontradas {fantasmaKeys.Count} claves de pesca fantasma");
                
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
                        Console.WriteLine($"[DEBUG] Guardando pesca fantasma: tipoArte={fantasma.tipoArte}");
                        await _fantasmaService.CreateAsync(fantasma);
                    }
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                AlertService.Error(TempData, $"Error al crear el registro: {ex.Message}");
                await CargarCatalogosAsync();
                return Page();
            }
        }

        private async Task CargarCatalogosAsync()
        {
            Pescadores = await _pescadorService.GetAllAsync();
            Embarcaciones = await _embarcacionService.GetAllAsync();
            SitiosPesca = await _sitioPescaService.GetAllAsync();
            ArtesPesca = await _artePescaService.GetAllAsync();
            Especies = (await _especieService.GetAllAsync()).Where(e => e.activo).ToList();
            Insumos = (await _insumoService.GetAllAsync()).Where(i => i.activo).ToList();
        }
    }
}
