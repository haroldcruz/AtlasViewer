using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface ISitioPescaService
{
 Task<List<SitioPesca>> GetAllAsync();
 Task<SitioPesca?> GetByIdAsync(string id);
 Task<SitioPesca?> GetByCodigoAsync(string codigo);
 Task CreateAsync(SitioPesca sitioPesca);
 Task UpdateAsync(string id, SitioPesca sitioPesca);
 Task<bool> DeleteAsync(string id);
}

public class SitioPescaService : ISitioPescaService
{
 private readonly IMongoCollection<SitioPesca> _sitiosPesca;

 public SitioPescaService(MongoService mongoService)
 {
 _sitiosPesca = mongoService.Database.GetCollection<SitioPesca>("sitiosPesca");
 }

 public async Task<List<SitioPesca>> GetAllAsync()
 {
 return await _sitiosPesca.Find(_ => true).ToListAsync();
 }

 public async Task<SitioPesca?> GetByIdAsync(string id)
 {
 return await _sitiosPesca.Find(s => s.Id == id).FirstOrDefaultAsync();
 }

 public async Task<SitioPesca?> GetByCodigoAsync(string codigo)
 {
 return await _sitiosPesca.Find(s => s.codigo == codigo).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(SitioPesca sitioPesca)
 {
 await _sitiosPesca.InsertOneAsync(sitioPesca);
 }

 public async Task UpdateAsync(string id, SitioPesca sitioPesca)
 {
 await _sitiosPesca.ReplaceOneAsync(s => s.Id == id, sitioPesca);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _sitiosPesca.DeleteOneAsync(s => s.Id == id);
 return result.DeletedCount > 0;
 }
}
