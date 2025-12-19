using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.VentasEspecies
{
    [Authorize(Roles = "Administrador,Editor")]
    public class CreateModel : PageModel
    {
        private readonly IVentaEspecieService _ventaEspecieService;
        private readonly IEspecieService _especieService;

        public CreateModel(IVentaEspecieService ventaEspecieService, IEspecieService especieService)
        {
            _ventaEspecieService = ventaEspecieService;
            _especieService = especieService;
        }

        [BindProperty]
        public VentaEspecie VentaEspecie { get; set; } = new();

        public List<Especie> Especies { get; set; } = new();

        public async Task OnGetAsync()
        {
            Especies = await _especieService.GetAllAsync();
            Especies = Especies.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            // Inicializar con fecha actual
            VentaEspecie.fecha = DateTime.Now;

            // Generar consecutivo automático
            var ventas = await _ventaEspecieService.GetAllAsync();
            var ultimoConsecutivo = ventas.Count;
            VentaEspecie.consecutivo = $"VE-{DateTime.Now:yyyyMMdd}-{(ultimoConsecutivo + 1).ToString("D4")}";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Cargar especies para el formulario en caso de error
            Especies = await _especieService.GetAllAsync();
            Especies = Especies.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            // Procesar detalle de venta desde el formulario
            VentaEspecie.detalle = new List<DetalleVentaEspecie>();
            decimal total = 0;

            var detalleKeys = Request.Form.Keys.Where(k => k.StartsWith("Detalle[") && k.EndsWith("].especieId"));

            foreach (var key in detalleKeys)
            {
                var indexStr = key.Replace("Detalle[", "").Replace("].especieId", "");
                if (int.TryParse(indexStr, out int index))
                {
                    var especieId = Request.Form[$"Detalle[{index}].especieId"].ToString();
                    var cantidadStr = Request.Form[$"Detalle[{index}].cantidadKg"].ToString();
                    var precioStr = Request.Form[$"Detalle[{index}].precioUnitario"].ToString();

                    if (!string.IsNullOrEmpty(especieId) &&
                        double.TryParse(cantidadStr, out double cantidad) &&
                        decimal.TryParse(precioStr, out decimal precio))
                    {
                        var subtotal = (decimal)cantidad * precio;

                        // Validar que hay suficiente inventario
                        var especie = await _especieService.GetByIdAsync(especieId);
                        if (especie == null)
                        {
                            ModelState.AddModelError("", $"Especie no encontrada.");
                            return Page();
                        }

                        if (especie.inventarioAcumuladoKg < cantidad)
                        {
                            ModelState.AddModelError("", $"Inventario insuficiente para {especie.nombre}. Disponible: {especie.inventarioAcumuladoKg} Kg");
                            return Page();
                        }

                        VentaEspecie.detalle.Add(new DetalleVentaEspecie
                        {
                            especieId = especieId,
                            cantidadKg = cantidad,
                            precioUnitario = precio,
                            subtotal = subtotal
                        });

                        total += subtotal;
                    }
                }
            }

            if (VentaEspecie.detalle.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos una especie a la venta.");
                return Page();
            }

            VentaEspecie.total = total;

            // Validar que el consecutivo no exista
            var existente = await _ventaEspecieService.GetByConsecutivoAsync(VentaEspecie.consecutivo);
            if (existente != null)
            {
                ModelState.AddModelError("VentaEspecie.consecutivo", "Ya existe una venta con este consecutivo.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Guardar la venta
            await _ventaEspecieService.CreateAsync(VentaEspecie);

            // Actualizar inventarios y precio promedio de venta
            foreach (var detalle in VentaEspecie.detalle)
            {
                var especie = await _especieService.GetByIdAsync(detalle.especieId!);
                if (especie != null)
                {
                    // Calcular nuevo precio promedio de venta ponderado
                    // (inventarioActual * precioPromedioActual) + (cantidadVendida * precioVenta) / inventarioTotal
                    double inventarioRestante = especie.inventarioAcumuladoKg - detalle.cantidadKg;
                    double inventarioTotal = especie.inventarioAcumuladoKg;
                    
                    if (inventarioTotal > 0)
                    {
                        decimal precioPromedioNuevo = ((decimal)inventarioRestante * especie.precioPromedioVenta + 
                                                        (decimal)detalle.cantidadKg * detalle.precioUnitario) / 
                                                       (decimal)inventarioTotal;
                        especie.precioPromedioVenta = precioPromedioNuevo;
                    }
                    
                    especie.inventarioAcumuladoKg = inventarioRestante;
                    await _especieService.UpdateAsync(especie.Id!, especie);
                }
            }

            AlertService.Success(TempData, "¡Venta registrada exitosamente!");
            return RedirectToPage("./Index");
        }
    }
}
