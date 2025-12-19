using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AtlasViewer.Pages.VentasEspecies
{
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
        private readonly IVentaEspecieService _ventaEspecieService;
        private readonly IEspecieService _especieService;

        public EditModel(IVentaEspecieService ventaEspecieService, IEspecieService especieService)
        {
            _ventaEspecieService = ventaEspecieService;
            _especieService = especieService;
        }

        [BindProperty]
        public VentaEspecie VentaEspecie { get; set; } = new();

        public List<Especie> Especies { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            VentaEspecie = await _ventaEspecieService.GetByIdAsync(id);

            if (VentaEspecie == null)
            {
                return NotFound();
            }

            Especies = await _especieService.GetAllAsync();
            Especies = Especies.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            // Cargar la venta original para comparar y restaurar inventarios
            var ventaOriginal = await _ventaEspecieService.GetByIdAsync(id);
            if (ventaOriginal == null)
            {
                return NotFound();
            }

            // Cargar especies para el formulario
            Especies = await _especieService.GetAllAsync();
            Especies = Especies.Where(e => e.activo).OrderBy(e => e.nombre).ToList();

            // Procesar nuevo detalle
            VentaEspecie.Id = id;
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
                VentaEspecie = ventaOriginal; // Restaurar para mostrar en la vista
                return Page();
            }

            VentaEspecie.total = total;

            if (!ModelState.IsValid)
            {
                VentaEspecie = ventaOriginal; // Restaurar para mostrar en la vista
                return Page();
            }

            // Restaurar inventarios originales
            foreach (var detalleOriginal in ventaOriginal.detalle)
            {
                var especie = await _especieService.GetByIdAsync(detalleOriginal.especieId!);
                if (especie != null)
                {
                    especie.inventarioAcumuladoKg += detalleOriginal.cantidadKg;
                    await _especieService.UpdateAsync(especie.Id!, especie);
                }
            }

            // Validar y descontar nuevos inventarios
            foreach (var detalleNuevo in VentaEspecie.detalle)
            {
                var especie = await _especieService.GetByIdAsync(detalleNuevo.especieId!);
                if (especie == null)
                {
                    ModelState.AddModelError("", $"Especie no encontrada.");
                    VentaEspecie = ventaOriginal;
                    
                    // Revertir cambios anteriores
                    foreach (var detalleOriginal in ventaOriginal.detalle)
                    {
                        var especieRevertir = await _especieService.GetByIdAsync(detalleOriginal.especieId!);
                        if (especieRevertir != null)
                        {
                            especieRevertir.inventarioAcumuladoKg -= detalleOriginal.cantidadKg;
                            await _especieService.UpdateAsync(especieRevertir.Id!, especieRevertir);
                        }
                    }
                    return Page();
                }

                if (especie.inventarioAcumuladoKg < detalleNuevo.cantidadKg)
                {
                    ModelState.AddModelError("", $"Inventario insuficiente para {especie.nombre}. Disponible: {especie.inventarioAcumuladoKg} Kg");
                    VentaEspecie = ventaOriginal;
                    
                    // Revertir cambios anteriores
                    foreach (var detalleOriginal in ventaOriginal.detalle)
                    {
                        var especieRevertir = await _especieService.GetByIdAsync(detalleOriginal.especieId!);
                        if (especieRevertir != null)
                        {
                            especieRevertir.inventarioAcumuladoKg -= detalleOriginal.cantidadKg;
                            await _especieService.UpdateAsync(especieRevertir.Id!, especieRevertir);
                        }
                    }
                    return Page();
                }

                // Calcular nuevo precio promedio de venta ponderado
                double inventarioRestante = especie.inventarioAcumuladoKg - detalleNuevo.cantidadKg;
                double inventarioTotal = especie.inventarioAcumuladoKg;
                
                if (inventarioTotal > 0)
                {
                    decimal precioPromedioNuevo = ((decimal)inventarioRestante * especie.precioPromedioVenta + 
                                                    (decimal)detalleNuevo.cantidadKg * detalleNuevo.precioUnitario) / 
                                                   (decimal)inventarioTotal;
                    especie.precioPromedioVenta = precioPromedioNuevo;
                }

                especie.inventarioAcumuladoKg = inventarioRestante;
                await _especieService.UpdateAsync(especie.Id!, especie);
            }

            // Actualizar la venta
            await _ventaEspecieService.UpdateAsync(id, VentaEspecie);

            AlertService.Success(TempData, "¡Venta actualizada exitosamente!");
            return RedirectToPage("./Index");
        }
    }
}
