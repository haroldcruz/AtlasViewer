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

        public async Task OnGetAsync()
        {
            Registros = await _registroCapturaService.GetAllAsync();

            // Cargar diccionarios para referencias
            var pescadores = await _pescadorService.GetAllAsync();
            PescadoresDict = pescadores.Where(p => p.Id != null).ToDictionary(p => p.Id!, p => p.nombre ?? "");

            var embarcaciones = await _embarcacionService.GetAllAsync();
            EmbarcacionesDict = embarcaciones.Where(e => e.Id != null).ToDictionary(e => e.Id!, e => e.matricula ?? e.nombre ?? "");

            var sitios = await _sitioPescaService.GetAllAsync();
            SitiosDict = sitios.Where(s => s.Id != null).ToDictionary(s => s.Id!, s => s.nombre ?? "");

            var artes = await _artePescaService.GetAllAsync();
            ArtesDict = artes.Where(a => a.Id != null).ToDictionary(a => a.Id!, a => a.nombre ?? "");
        }
    }
}
