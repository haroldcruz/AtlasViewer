using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IRegistroCapturaService
{
 Task<List<RegistroCaptura>> GetAllAsync();
 Task<RegistroCaptura?> GetByIdAsync(string id);
 Task<List<RegistroCaptura>> GetByPescadorAsync(string pescadorId);
 Task<List<RegistroCaptura>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
 Task CreateAsync(RegistroCaptura registro);
 Task UpdateAsync(string id, RegistroCaptura registro);
 Task<bool> DeleteAsync(string id);
}

public class RegistroCapturaService : IRegistroCapturaService
{
 private readonly IMongoCollection<RegistroCaptura> _registros;

 public RegistroCapturaService(MongoService mongoService)
 {
 _registros = mongoService.Database.GetCollection<RegistroCaptura>("registroCaptura");
 }

 public async Task<List<RegistroCaptura>> GetAllAsync()
 {
 return await _registros.Find(_ => true).ToListAsync();
 }

 public async Task<RegistroCaptura?> GetByIdAsync(string id)
 {
 return await _registros.Find(r => r.Id == id).FirstOrDefaultAsync();
 }

 public async Task<List<RegistroCaptura>> GetByPescadorAsync(string pescadorId)
 {
 return await _registros.Find(r => r.pescadorId == pescadorId).ToListAsync();
 }

 public async Task<List<RegistroCaptura>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
 {
 return await _registros.Find(r => r.fecha >= fechaInicio && r.fecha <= fechaFin).ToListAsync();
 }

 public async Task CreateAsync(RegistroCaptura registro)
 {
 await _registros.InsertOneAsync(registro);
 }

 public async Task UpdateAsync(string id, RegistroCaptura registro)
 {
 await _registros.ReplaceOneAsync(r => r.Id == id, registro);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _registros.DeleteOneAsync(r => r.Id == id);
 return result.DeletedCount > 0;
 }
}
