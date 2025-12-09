using Microsoft.AspNetCore.Mvc;
using AtlasViewer.Services;

namespace AtlasViewer.Controllers
{
 public class ItemsController : Controller
 {
 private readonly MongoService _mongoService;

 public ItemsController(MongoService mongoService)
 {
 _mongoService = mongoService;
 }

 public async Task<IActionResult> Index()
 {
 var items = await _mongoService.GetItemsAsync();
 return View(items);
 }
 }
}
