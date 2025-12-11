using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IPescaIncidentalService
{
 Task<List<PescaIncidental>> GetAllAsync();
 Task<PescaIncidental?> GetByIdAsync(string id);
 Task<List<PescaIncidental>> GetByRegistroAsync(string registroId);
 Task<List<PescaIncidental>> GetByEspecieAsync(string especieId);
 Task CreateAsync(PescaIncidental pesca);
 Task UpdateAsync(string id, PescaIncidental pesca);
 Task<bool> DeleteAsync(string id);
}

public class PescaIncidentalService : IPescaIncidentalService
{
 private readonly IMongoCollection<PescaIncidental> _pescas;

 public PescaIncidentalService(MongoService mongoService)
 {
 _pescas = mongoService.Database.GetCollection<PescaIncidental>("pescaIncidental");
 }

 public async Task<List<PescaIncidental>> GetAllAsync()
 {
 return await _pescas.Find(_ => true).ToListAsync();
 }

 public async Task<PescaIncidental?> GetByIdAsync(string id)
 {
 return await _pescas.Find(p => p.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<PescaIncidental>> GetByRegistroAsync(string registroId)
 {
 return await _pescas.Find(p => p.registroId == registroId).ToListAsync();
 }

 public async Task<List<PescaIncidental>> GetByEspecieAsync(string especieId)
 {
 return await _pescas.Find(p => p.especieId == especieId).ToListAsync();
 }

 public async Task CreateAsync(PescaIncidental pesca)
 {
 await _pescas.InsertOneAsync(pesca);
 }

 public async Task UpdateAsync(string id, PescaIncidental pesca)
 {
 await _pescas.ReplaceOneAsync(p => p.Id == id, pesca);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _pescas.DeleteOneAsync(p => p.Id == id);
 return result.DeletedCount > 0;
 }
}
