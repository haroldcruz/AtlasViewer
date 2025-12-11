using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IEmbarcacionService
{
 Task<List<Embarcacion>> GetAllAsync();
 Task<Embarcacion?> GetByIdAsync(string id);
 Task CreateAsync(Embarcacion embarcacion);
 Task UpdateAsync(string id, Embarcacion embarcacion);
 Task<bool> DeleteAsync(string id);
}

public class EmbarcacionService : IEmbarcacionService
{
 private readonly IMongoCollection<Embarcacion> _embarcaciones;

 public EmbarcacionService(MongoService mongoService)
 {
 _embarcaciones = mongoService.Database.GetCollection<Embarcacion>("embarcaciones");
 }

 public async Task<List<Embarcacion>> GetAllAsync()
 {
 return await _embarcaciones.Find(_ => true).ToListAsync();
 }

 public async Task<Embarcacion?> GetByIdAsync(string id)
 {
 return await _embarcaciones.Find(e => e.Id == id).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(Embarcacion embarcacion)
 {
 await _embarcaciones.InsertOneAsync(embarcacion);
 }

 public async Task UpdateAsync(string id, Embarcacion embarcacion)
 {
 await _embarcaciones.ReplaceOneAsync(e => e.Id == id, embarcacion);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _embarcaciones.DeleteOneAsync(e => e.Id == id);
 return result.DeletedCount > 0;
 }
}
