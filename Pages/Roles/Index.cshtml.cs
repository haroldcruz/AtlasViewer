using AtlasViewer.Models;
using AtlasViewer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtlasViewer.Pages.Roles
{
 public class IndexModel : PageModel
 {
 private readonly IRolService _roles;
 public IndexModel(IRolService roles) { _roles = roles; }
 public IList<Rol> Roles { get; private set; } = new List<Rol>();
 public async Task OnGetAsync() { Roles = await _roles.GetRolesAsync(); }
 }
}
