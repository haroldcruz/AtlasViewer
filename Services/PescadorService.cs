using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IPescadorService
{
 Task<List<Pescador>> GetAllAsync();
 Task<Pescador?> GetByIdAsync(string id);
 Task CreateAsync(Pescador pescador);
 Task UpdateAsync(string id, Pescador pescador);
 Task<bool> DeleteAsync(string id);
}

public class PescadorService : IPescadorService
{
 private readonly IMongoCollection<Pescador> _pescadores;

 public PescadorService(MongoService mongoService)
 {
 _pescadores = mongoService.Database.GetCollection<Pescador>("pescadores");
 }

 public async Task<List<Pescador>> GetAllAsync()
 {
 return await _pescadores.Find(_ => true).ToListAsync();
 }

 public async Task<Pescador?> GetByIdAsync(string id)
 {
 return await _pescadores.Find(p => p.Id == id).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(Pescador pescador)
 {
 await _pescadores.InsertOneAsync(pescador);
 }

 public async Task UpdateAsync(string id, Pescador pescador)
 {
 await _pescadores.ReplaceOneAsync(p => p.Id == id, pescador);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _pescadores.DeleteOneAsync(p => p.Id == id);
 return result.DeletedCount > 0;
 }
}
