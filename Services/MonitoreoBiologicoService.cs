using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IMonitoreoBiologicoService
{
 Task<List<MonitoreoBiologico>> GetAllAsync();
 Task<MonitoreoBiologico?> GetByIdAsync(string id);
 Task<List<MonitoreoBiologico>> GetByRegistroAsync(string registroId);
 Task<List<MonitoreoBiologico>> GetByEspecieAsync(string especieId);
 Task CreateAsync(MonitoreoBiologico monitoreo);
 Task UpdateAsync(string id, MonitoreoBiologico monitoreo);
 Task<bool> DeleteAsync(string id);
}

public class MonitoreoBiologicoService : IMonitoreoBiologicoService
{
 private readonly IMongoCollection<MonitoreoBiologico> _monitoreos;

 public MonitoreoBiologicoService(MongoService mongoService)
 {
 _monitoreos = mongoService.Database.GetCollection<MonitoreoBiologico>("monitoreoBiologico");
 }

 public async Task<List<MonitoreoBiologico>> GetAllAsync()
 {
 return await _monitoreos.Find(_ => true).ToListAsync();
 }

 public async Task<MonitoreoBiologico?> GetByIdAsync(string id)
 {
 return await _monitoreos.Find(m => m.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<MonitoreoBiologico>> GetByRegistroAsync(string registroId)
 {
 return await _monitoreos.Find(m => m.registroId == registroId).ToListAsync();
 }

 public async Task<List<MonitoreoBiologico>> GetByEspecieAsync(string especieId)
 {
 return await _monitoreos.Find(m => m.especieId == especieId).ToListAsync();
 }

 public async Task CreateAsync(MonitoreoBiologico monitoreo)
 {
 await _monitoreos.InsertOneAsync(monitoreo);
 }

 public async Task UpdateAsync(string id, MonitoreoBiologico monitoreo)
 {
 await _monitoreos.ReplaceOneAsync(m => m.Id == id, monitoreo);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _monitoreos.DeleteOneAsync(m => m.Id == id);
 return result.DeletedCount > 0;
 }
}
