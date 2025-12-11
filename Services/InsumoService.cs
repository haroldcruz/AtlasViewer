using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IInsumoService
{
 Task<List<Insumo>> GetAllAsync();
 Task<Insumo?> GetByIdAsync(string id);
 Task<Insumo?> GetByCodigoAsync(string codigo);
 Task CreateAsync(Insumo insumo);
 Task UpdateAsync(string id, Insumo insumo);
 Task<bool> DeleteAsync(string id);
}

public class InsumoService : IInsumoService
{
 private readonly IMongoCollection<Insumo> _insumos;

 public InsumoService(MongoService mongoService)
 {
 _insumos = mongoService.Database.GetCollection<Insumo>("insumos");
 }

 public async Task<List<Insumo>> GetAllAsync()
 {
 return await _insumos.Find(_ => true).ToListAsync();
 }

 public async Task<Insumo?> GetByIdAsync(string id)
 {
 return await _insumos.Find(i => i.Id == id).FirstOrDefaultAsync();
 }

 public async Task<Insumo?> GetByCodigoAsync(string codigo)
 {
 return await _insumos.Find(i => i.codigo == codigo).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(Insumo insumo)
 {
 await _insumos.InsertOneAsync(insumo);
 }

 public async Task UpdateAsync(string id, Insumo insumo)
 {
 await _insumos.ReplaceOneAsync(i => i.Id == id, insumo);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _insumos.DeleteOneAsync(i => i.Id == id);
 return result.DeletedCount > 0;
 }
}
