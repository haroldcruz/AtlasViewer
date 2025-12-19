using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.RegistrosCapturas
{
    public class IndexModel : PageModel
    {
        private readonly IRegistroCapturaService _registroCapturaService;
        private readonly IPescadorService _pescadorService;
        private readonly IEmbarcacionService _embarcacionService;
        private readonly ISitioPescaService _sitioPescaService;
        private readonly IArtePescaService _artePescaService;

        public IndexModel(
            IRegistroCapturaService registroCapturaService,
            IPescadorService pescadorService,
            IEmbarcacionService embarcacionService,
            ISitioPescaService sitioPescaService,
            IArtePescaService artePescaService)
        {
            _registroCapturaService = registroCapturaService;
            _pescadorService = pescadorService;
            _embarcacionService = embarcacionService;
            _sitioPescaService = sitioPescaService;
            _artePescaService = artePescaService;
        }

        public List<RegistroCaptura> Registros { get; set; } = new List<RegistroCaptura>();
        public Dictionary<string, string> PescadoresDict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> EmbarcacionesDict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> SitiosDict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ArtesDict { get; set; } = new Dictionary<string, string>();
        public string? BusquedaPescador { get; set; }
        public DateTime? BusquedaFechaDesde { get; set; }
        public DateTime? BusquedaFechaHasta { get; set; }
        public bool MostrarTodos { get; set; }

        public async Task OnGetAsync(string? busquedaPescador = null, DateTime? busquedaFechaDesde = null, DateTime? busquedaFechaHasta = null, bool mostrarTodos = false)
        {
            BusquedaPescador = busquedaPescador;
            BusquedaFechaDesde = busquedaFechaDesde;
            BusquedaFechaHasta = busquedaFechaHasta;
            MostrarTodos = mostrarTodos;

            var todosRegistros = await _registroCapturaService.GetAllAsync();
            
            // Cargar diccionarios para referencias
            var pescadores = await _pescadorService.GetAllAsync();
            PescadoresDict = pescadores.Where(p => p.Id != null).ToDictionary(p => p.Id!, p => p.nombre ?? "");

            var embarcaciones = await _embarcacionService.GetAllAsync();
            EmbarcacionesDict = embarcaciones.Where(e => e.Id != null).ToDictionary(e => e.Id!, e => e.nombre ?? e.matricula ?? "");

            var sitios = await _sitioPescaService.GetAllAsync();
            SitiosDict = sitios.Where(s => s.Id != null).ToDictionary(s => s.Id!, s => s.nombre ?? "");

            var artes = await _artePescaService.GetAllAsync();
            ArtesDict = artes.Where(a => a.Id != null).ToDictionary(a => a.Id!, a => a.nombre ?? "");
            
            // Aplicar filtros
            var registrosFiltrados = todosRegistros.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(busquedaPescador))
            {
                // Buscar por nombre de pescador
                var pescadoresCoincidentes = pescadores
                    .Where(p => p.nombre != null && p.nombre.Contains(busquedaPescador, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToList();
                    
                if (pescadoresCoincidentes.Any())
                {
                    registrosFiltrados = registrosFiltrados.Where(r => 
                        !string.IsNullOrEmpty(r.pescadorId) && 
                        pescadoresCoincidentes.Contains(r.pescadorId));
                }
                else
                {
                    // Si no hay coincidencias, devolver lista vacía
                    Registros = new List<RegistroCaptura>();
                    return;
                }
            }
            
            if (busquedaFechaDesde.HasValue)
            {
                registrosFiltrados = registrosFiltrados.Where(r => r.fecha >= busquedaFechaDesde.Value);
            }
            
            if (busquedaFechaHasta.HasValue)
            {
                registrosFiltrados = registrosFiltrados.Where(r => r.fecha <= busquedaFechaHasta.Value);
            }
            
            // Ordenar por fecha descendente
            registrosFiltrados = registrosFiltrados.OrderByDescending(r => r.fecha).ThenByDescending(r => r.hora);
            
            // Limitar a 10 últimos si no se pide mostrar todos
            if (!mostrarTodos && string.IsNullOrWhiteSpace(busquedaPescador) && !busquedaFechaDesde.HasValue && !busquedaFechaHasta.HasValue)
            {
                Registros = registrosFiltrados.Take(10).ToList();
            }
            else
            {
                Registros = registrosFiltrados.ToList();
            }
        }
    }
}
