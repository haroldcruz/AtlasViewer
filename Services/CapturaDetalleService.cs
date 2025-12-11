using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface ICapturaDetalleService
{
 Task<List<CapturaDetalle>> GetAllAsync();
 Task<CapturaDetalle?> GetByIdAsync(string id);
 Task<List<CapturaDetalle>> GetByRegistroAsync(string registroId);
 Task<List<CapturaDetalle>> GetByEspecieAsync(string especieId);
 Task CreateAsync(CapturaDetalle captura);
 Task UpdateAsync(string id, CapturaDetalle captura);
 Task<bool> DeleteAsync(string id);
}

public class CapturaDetalleService : ICapturaDetalleService
{
 private readonly IMongoCollection<CapturaDetalle> _capturas;

 public CapturaDetalleService(MongoService mongoService)
 {
 _capturas = mongoService.Database.GetCollection<CapturaDetalle>("capturasDetalle");
 }

 public async Task<List<CapturaDetalle>> GetAllAsync()
 {
 return await _capturas.Find(_ => true).ToListAsync();
 }

 public async Task<CapturaDetalle?> GetByIdAsync(string id)
 {
 return await _capturas.Find(c => c.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<CapturaDetalle>> GetByRegistroAsync(string registroId)
 {
 return await _capturas.Find(c => c.registroId == registroId).ToListAsync();
 }

 public async Task<List<CapturaDetalle>> GetByEspecieAsync(string especieId)
 {
 return await _capturas.Find(c => c.especieId == especieId).ToListAsync();
 }

 public async Task CreateAsync(CapturaDetalle captura)
 {
 await _capturas.InsertOneAsync(captura);
 }

 public async Task UpdateAsync(string id, CapturaDetalle captura)
 {
 await _capturas.ReplaceOneAsync(c => c.Id == id, captura);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _capturas.DeleteOneAsync(c => c.Id == id);
 return result.DeletedCount > 0;
 }
}
