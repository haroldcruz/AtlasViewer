using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IEspecieService
{
 Task<List<Especie>> GetAllAsync();
 Task<Especie?> GetByIdAsync(string id);
 Task<Especie?> GetByCodigoAsync(string codigo);
 Task CreateAsync(Especie especie);
 Task UpdateAsync(string id, Especie especie);
 Task<bool> DeleteAsync(string id);
}

public class EspecieService : IEspecieService
{
 private readonly IMongoCollection<Especie> _especies;

 public EspecieService(MongoService mongoService)
 {
 _especies = mongoService.Database.GetCollection<Especie>("especies");
 }

 public async Task<List<Especie>> GetAllAsync()
 {
 return await _especies.Find(_ => true).ToListAsync();
 }

 public async Task<Especie?> GetByIdAsync(string id)
 {
 return await _especies.Find(e => e.Id == id).FirstOrDefaultAsync();
 }

 public async Task<Especie?> GetByCodigoAsync(string codigo)
 {
 return await _especies.Find(e => e.codigo == codigo).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(Especie especie)
 {
 await _especies.InsertOneAsync(especie);
 }

 public async Task UpdateAsync(string id, Especie especie)
 {
 await _especies.ReplaceOneAsync(e => e.Id == id, especie);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _especies.DeleteOneAsync(e => e.Id == id);
 return result.DeletedCount > 0;
 }
}
