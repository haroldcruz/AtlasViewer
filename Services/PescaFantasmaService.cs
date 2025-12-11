using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IPescaFantasmaService
{
 Task<List<PescaFantasma>> GetAllAsync();
 Task<PescaFantasma?> GetByIdAsync(string id);
 Task<List<PescaFantasma>> GetByRegistroAsync(string registroId);
 Task CreateAsync(PescaFantasma pesca);
 Task UpdateAsync(string id, PescaFantasma pesca);
 Task<bool> DeleteAsync(string id);
}

public class PescaFantasmaService : IPescaFantasmaService
{
 private readonly IMongoCollection<PescaFantasma> _pescas;

 public PescaFantasmaService(MongoService mongoService)
 {
 _pescas = mongoService.Database.GetCollection<PescaFantasma>("pescaFantasma");
 }

 public async Task<List<PescaFantasma>> GetAllAsync()
 {
 return await _pescas.Find(_ => true).ToListAsync();
 }

 public async Task<PescaFantasma?> GetByIdAsync(string id)
 {
 return await _pescas.Find(p => p.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<PescaFantasma>> GetByRegistroAsync(string registroId)
 {
 return await _pescas.Find(p => p.registroId == registroId).ToListAsync();
 }

 public async Task CreateAsync(PescaFantasma pesca)
 {
 await _pescas.InsertOneAsync(pesca);
 }

 public async Task UpdateAsync(string id, PescaFantasma pesca)
 {
 await _pescas.ReplaceOneAsync(p => p.Id == id, pesca);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _pescas.DeleteOneAsync(p => p.Id == id);
 return result.DeletedCount > 0;
 }
}
