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

        public CreateModel(
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
        public RegistroCaptura RegistroCaptura { get; set; } = new RegistroCaptura();

        public List<Pescador> Pescadores { get; set; } = new List<Pescador>();
        public List<Embarcacion> Embarcaciones { get; set; } = new List<Embarcacion>();
        public List<SitioPesca> SitiosPesca { get; set; } = new List<SitioPesca>();
        public List<Models.ArtePesca> ArtesPesca { get; set; } = new List<Models.ArtePesca>();
        public List<Especie> Especies { get; set; } = new List<Especie>();
        public List<Insumo> Insumos { get; set; } = new List<Insumo>();

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
                    var captura = new CapturaDetalle
                    {
                        especieId = especieId,
                        numeroPeces = int.Parse(Request.Form[$"Capturas[{indexStr}].numeroPeces"].ToString() ?? "0"),
                        pesoTotalKg = double.Parse(Request.Form[$"Capturas[{indexStr}].pesoTotalKg"].ToString() ?? "0"),
                        precioKilo = decimal.Parse(Request.Form[$"Capturas[{indexStr}].precioKilo"].ToString() ?? "0"),
                        total = decimal.Parse(Request.Form[$"Capturas[{indexStr}].total"].ToString() ?? "0"),
                        insumos = new List<AlistoInsumo>()
                    };
                    RegistroCaptura.capturas.Add(captura);
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
                    var insumo = new AlistoInsumo
                    {
                        insumoId = insumoId,
                        cantidad = double.Parse(Request.Form[$"Insumos[{indexStr}].cantidad"].ToString() ?? "0"),
                        precioUnitario = decimal.Parse(Request.Form[$"Insumos[{indexStr}].precioUnitario"].ToString() ?? "0"),
                        subtotal = decimal.Parse(Request.Form[$"Insumos[{indexStr}].subtotal"].ToString() ?? "0")
                    };
                    insumosGlobales.Add(insumo);
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
                ModelState.AddModelError("", "Debe registrar al menos una captura");
            }

            if (!ModelState.IsValid)
            {
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
