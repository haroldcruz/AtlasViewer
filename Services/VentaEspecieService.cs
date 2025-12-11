using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IVentaEspecieService
{
 Task<List<VentaEspecie>> GetAllAsync();
 Task<VentaEspecie?> GetByIdAsync(string id);
 Task<VentaEspecie?> GetByConsecutivoAsync(string consecutivo);
 Task<List<VentaEspecie>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
 Task CreateAsync(VentaEspecie venta);
 Task UpdateAsync(string id, VentaEspecie venta);
 Task<bool> DeleteAsync(string id);
}

public class VentaEspecieService : IVentaEspecieService
{
 private readonly IMongoCollection<VentaEspecie> _ventas;

 public VentaEspecieService(MongoService mongoService)
 {
 _ventas = mongoService.Database.GetCollection<VentaEspecie>("ventasEspecies");
 }

 public async Task<List<VentaEspecie>> GetAllAsync()
 {
 return await _ventas.Find(_ => true).ToListAsync();
 }

 public async Task<VentaEspecie?> GetByIdAsync(string id)
 {
 return await _ventas.Find(v => v.Id == id).FirstOrDefaultAsync();
 }

 public async Task<VentaEspecie?> GetByConsecutivoAsync(string consecutivo)
 {
 return await _ventas.Find(v => v.consecutivo == consecutivo).FirstOrDefaultAsync();
 }

 public async Task<List<VentaEspecie>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
 {
 return await _ventas.Find(v => v.fecha >= fechaInicio && v.fecha <= fechaFin).ToListAsync();
 }

 public async Task CreateAsync(VentaEspecie venta)
 {
 await _ventas.InsertOneAsync(venta);
 }

 public async Task UpdateAsync(string id, VentaEspecie venta)
 {
 await _ventas.ReplaceOneAsync(v => v.Id == id, venta);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _ventas.DeleteOneAsync(v => v.Id == id);
 return result.DeletedCount > 0;
 }
}
