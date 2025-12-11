using AtlasViewer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace AtlasViewer.Services;

public interface IRolService
{
 Task<IList<Rol>> GetRolesAsync(CancellationToken cancellationToken = default);
 Task<Rol?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
 Task<Rol?> GetByCodigoAsync(int codigo, CancellationToken cancellationToken = default);
 Task<Rol> CreateAsync(Rol nuevo, CancellationToken cancellationToken = default);
 Task UpdateAsync(Rol rol, IUsuarioService usuarioService, CancellationToken cancellationToken = default);
 Task<bool> DeleteAsync(string id, IUsuarioService usuarioService, CancellationToken cancellationToken = default);
}

public class RolService : IRolService
{
 private readonly IMongoCollection<Rol> _collection;
 private readonly IMongoClient _client;

 public RolService(IOptions<MongoSettings> options)
 {
 var cfg = options.Value;
 var settings = MongoClientSettings.FromConnectionString(cfg.ConnectionString);
 settings.ServerApi = new ServerApi(ServerApiVersion.V1);
 _client = new MongoClient(settings);
 var db = _client.GetDatabase(cfg.Database);
 _collection = db.GetCollection<Rol>("Roles");
 }

 public async Task<IList<Rol>> GetRolesAsync(CancellationToken cancellationToken = default)
 {
 var cursor = await _collection.FindAsync(FilterDefinition<Rol>.Empty, cancellationToken: cancellationToken);
 return await cursor.ToListAsync(cancellationToken);
 }
 public async Task<Rol?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
 {
 var cursor = await _collection.FindAsync(r => r.Id == id, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }
 public async Task<Rol?> GetByCodigoAsync(int codigo, CancellationToken cancellationToken = default)
 {
 var cursor = await _collection.FindAsync(r => r.id_rol == codigo, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }
 public async Task<Rol> CreateAsync(Rol nuevo, CancellationToken cancellationToken = default)
 {
 nuevo.Id ??= ObjectId.GenerateNewId().ToString();
 await _collection.InsertOneAsync(nuevo, cancellationToken: cancellationToken);
 return nuevo;
 }
 public async Task UpdateAsync(Rol rol, IUsuarioService usuarioService, CancellationToken cancellationToken = default)
 {
 // Obtener el rol anterior para ver si cambió el id_rol
 var rolAnterior = await GetByIdAsync(rol.Id!);
 if (rolAnterior != null && rolAnterior.id_rol != rol.id_rol)
 {
 // Actualizar en cascada todos los usuarios que tenían el rol anterior
 await usuarioService.UpdateRolEnCascadaAsync(rolAnterior.id_rol, rol.id_rol, cancellationToken);
 }
 await _collection.ReplaceOneAsync(r => r.Id == rol.Id, rol, cancellationToken: cancellationToken);
 }
 public async Task<bool> DeleteAsync(string id, IUsuarioService usuarioService, CancellationToken cancellationToken = default)
 {
 // Verificar si el rol está en uso
 var rol = await GetByIdAsync(id, cancellationToken);
 if (rol == null) return false;
 
 var usuariosConRol = await usuarioService.CountByRolAsync(rol.id_rol, cancellationToken);
 if (usuariosConRol > 0)
 {
 return false; // No se puede eliminar, hay usuarios con este rol
 }
 
 await _collection.DeleteOneAsync(r => r.Id == id, cancellationToken);
 return true;
 }
}
