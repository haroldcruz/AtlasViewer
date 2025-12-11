using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface ICompraInsumoService
{
 Task<List<CompraInsumo>> GetAllAsync();
 Task<CompraInsumo?> GetByIdAsync(string id);
 Task<List<CompraInsumo>> GetByProveedorAsync(string proveedorId);
 Task<List<CompraInsumo>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
 Task CreateAsync(CompraInsumo compra);
 Task UpdateAsync(string id, CompraInsumo compra);
 Task<bool> DeleteAsync(string id);
}

public class CompraInsumoService : ICompraInsumoService
{
 private readonly IMongoCollection<CompraInsumo> _compras;

 public CompraInsumoService(MongoService mongoService)
 {
 _compras = mongoService.Database.GetCollection<CompraInsumo>("comprasInsumos");
 }

 public async Task<List<CompraInsumo>> GetAllAsync()
 {
 return await _compras.Find(_ => true).ToListAsync();
 }

 public async Task<CompraInsumo?> GetByIdAsync(string id)
 {
 return await _compras.Find(c => c.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<CompraInsumo>> GetByProveedorAsync(string proveedorId)
 {
 return await _compras.Find(c => c.proveedorId == proveedorId).ToListAsync();
 }

 public async Task<List<CompraInsumo>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
 {
 return await _compras.Find(c => c.fecha >= fechaInicio && c.fecha <= fechaFin).ToListAsync();
 }

 public async Task CreateAsync(CompraInsumo compra)
 {
 await _compras.InsertOneAsync(compra);
 }

 public async Task UpdateAsync(string id, CompraInsumo compra)
 {
 await _compras.ReplaceOneAsync(c => c.Id == id, compra);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _compras.DeleteOneAsync(c => c.Id == id);
 return result.DeletedCount > 0;
 }
}
