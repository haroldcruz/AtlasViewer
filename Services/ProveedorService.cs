using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IProveedorService
{
 Task<List<Proveedor>> GetAllAsync();
 Task<Proveedor?> GetByIdAsync(string id);
 Task CreateAsync(Proveedor proveedor);
 Task UpdateAsync(string id, Proveedor proveedor);
 Task<bool> DeleteAsync(string id);
}

public class ProveedorService : IProveedorService
{
 private readonly IMongoCollection<Proveedor> _proveedores;

 public ProveedorService(MongoService mongoService)
 {
 _proveedores = mongoService.Database.GetCollection<Proveedor>("proveedores");
 }

 public async Task<List<Proveedor>> GetAllAsync()
 {
 return await _proveedores.Find(_ => true).ToListAsync();
 }

 public async Task<Proveedor?> GetByIdAsync(string id)
 {
 return await _proveedores.Find(p => p.Id == id).FirstOrDefaultAsync();
 }

 public async Task CreateAsync(Proveedor proveedor)
 {
 await _proveedores.InsertOneAsync(proveedor);
 }

 public async Task UpdateAsync(string id, Proveedor proveedor)
 {
 await _proveedores.ReplaceOneAsync(p => p.Id == id, proveedor);
 }

 public async Task<bool> DeleteAsync(string id)
 {
 var result = await _proveedores.DeleteOneAsync(p => p.Id == id);
 return result.DeletedCount > 0;
 }
}
