using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace AtlasViewer.Pages.ComprasInsumos
{
    [Authorize(Roles = "Administrador,Editor")]
    public class CreateModel : PageModel
    {
        private readonly ICompraInsumoService _compraInsumoService;
        private readonly IProveedorService _proveedorService;
        private readonly IInsumoService _insumoService;

        public CreateModel(
            ICompraInsumoService compraInsumoService, 
            IProveedorService proveedorService,
            IInsumoService insumoService)
        {
            _compraInsumoService = compraInsumoService;
            _proveedorService = proveedorService;
            _insumoService = insumoService;
        }

        [BindProperty]
        public CompraInsumo CompraInsumo { get; set; } = new CompraInsumo();

        [BindProperty]
        public string DetalleJson { get; set; } = string.Empty;

        public List<Proveedor> Proveedores { get; set; } = new();
        public List<Insumo> Insumos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Proveedores = await _proveedorService.GetAllAsync();
            Proveedores = Proveedores.Where(p => p.activo).OrderBy(p => p.nombre).ToList();

            Insumos = await _insumoService.GetAllAsync();
            Insumos = Insumos.Where(i => i.activo).OrderBy(i => i.codigo).ToList();

            CompraInsumo.fecha = DateTime.Now;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                if (!string.IsNullOrEmpty(DetalleJson))
                {
                    var detalleList = JsonSerializer.Deserialize<List<DetalleCompraInsumo>>(DetalleJson);
                    if (detalleList != null && detalleList.Count > 0)
                    {
                        CompraInsumo.detalle = detalleList;
                    }
                }

                if (CompraInsumo.detalle.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Debe agregar al menos un insumo a la compra");
                    await OnGetAsync();
                    return Page();
                }

                // Calcular total
                CompraInsumo.total = CompraInsumo.detalle.Sum(d => d.subtotal);

                // Crear la compra
                await _compraInsumoService.CreateAsync(CompraInsumo);

                // Actualizar inventario y precios de cada insumo
                foreach (var detalle in CompraInsumo.detalle)
                {
                    if (!string.IsNullOrEmpty(detalle.insumoId))
                    {
                        var insumo = await _insumoService.GetByIdAsync(detalle.insumoId);
                        if (insumo != null)
                        {
                            // Calcular nuevo precio promedio ponderado de compra
                            var inventarioAnterior = insumo.inventarioActual;
                            var costoTotalAnterior = inventarioAnterior * (double)insumo.precioPromedioCompra;
                            var costoTotalNuevo = detalle.cantidad * (double)detalle.costoUnitario;
                            var inventarioNuevo = inventarioAnterior + detalle.cantidad;

                            if (inventarioNuevo > 0)
                            {
                                insumo.precioPromedioCompra = (decimal)((costoTotalAnterior + costoTotalNuevo) / inventarioNuevo);
                            }
                            else
                            {
                                insumo.precioPromedioCompra = detalle.costoUnitario;
                            }

                            // Calcular precio de venta con porcentaje de ganancia
                            var porcentajeGanancia = 25m; // Por defecto 25%
                            if (Request.Form.ContainsKey("porcentajeGanancia"))
                            {
                                if (decimal.TryParse(Request.Form["porcentajeGanancia"], out var porcentaje))
                                {
                                    porcentajeGanancia = porcentaje;
                                }
                            }

                            insumo.precioPromedioVenta = insumo.precioPromedioCompra * (1 + (porcentajeGanancia / 100));

                            // Actualizar inventario
                            insumo.inventarioActual = inventarioNuevo;

                            // Guardar cambios
                            await _insumoService.UpdateAsync(insumo.Id!, insumo);
                        }
                    }
                }

                AlertService.Success(TempData, $"Compra registrada exitosamente. Total: ₡{CompraInsumo.total:N2}");
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al procesar la compra: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }
}
