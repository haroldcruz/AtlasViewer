using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.RegistrosCapturas
{
    public class DetailsModel : PageModel
    {
        private readonly IRegistroCapturaService _registroCapturaService;
        private readonly IPescadorService _pescadorService;
        private readonly IEmbarcacionService _embarcacionService;
        private readonly ISitioPescaService _sitioPescaService;
        private readonly IArtePescaService _artePescaService;
        private readonly IEspecieService _especieService;
        private readonly IInsumoService _insumoService;

        public DetailsModel(
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

        public RegistroCaptura? RegistroCaptura { get; set; }
        public string PescadorNombre { get; set; } = string.Empty;
        public string EmbarcacionNombre { get; set; } = string.Empty;
        public string SitioNombre { get; set; } = string.Empty;
        public string ArteNombre { get; set; } = string.Empty;
        public Dictionary<string, string> EspeciesDict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> InsumosDict { get; set; } = new Dictionary<string, string>();

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

            // Cargar diccionarios
            var especies = await _especieService.GetAllAsync();
            EspeciesDict = especies.Where(e => e.Id != null).ToDictionary(e => e.Id!, e => e.nombre ?? "");

            var insumos = await _insumoService.GetAllAsync();
            InsumosDict = insumos.Where(i => i.Id != null).ToDictionary(i => i.Id!, i => $"{i.nombre} ({i.unidad})");

            return Page();
        }
    }
}
