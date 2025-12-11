using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IArtePescaService
{
 Task<List<ArtePesca>> GetAllAsync();
 Task<ArtePesca?> GetByIdAsync(string id);
 Task<ArtePesca?> GetByCodigoAsync(string codigo);
 Task CreateAsync(ArtePesca artePesca);
 Task UpdateAsync(string id, ArtePesca artePesca);
 Task<bool> DeleteAsync(string id);
}

public class ArtePescaService : IArtePescaService
{
 private readonly IMongoCollection<ArtePesca> _artesPesca;

 public ArtePescaService(MongoService mongoService)
 {
 _artesPesca = mongoService.Database.GetCollection<ArtePesca>("artePesca");
 }

 public async Task<List<ArtePesca>> GetAllAsync()
 {
 return await _artesPesca.Find(_ => true).ToListAsync();
 }

 public async Task<ArtePesca?> GetByIdAsync(string id)
 {
 return await _artesPesca.Find(a => a.Id == id).FirstOrDefaultAsync();
 }

 public async Task<ArtePesca?> GetByCodigoAsync(string codigo)
 {
 return await _artesPesca.Find(a => a.codigo == codigo).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(ArtePesca artePesca)
 {
 await _artesPesca.InsertOneAsync(artePesca);
 }

 public async Task UpdateAsync(string id, ArtePesca artePesca)
 {
 await _artesPesca.ReplaceOneAsync(a => a.Id == id, artePesca);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _artesPesca.DeleteOneAsync(a => a.Id == id);
 return result.DeletedCount > 0;
 }
}
