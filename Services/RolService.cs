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
 Task UpdateAsync(Rol rol, CancellationToken cancellationToken = default);
 Task DeleteAsync(string id, CancellationToken cancellationToken = default);
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
 var cursor = await _collection.FindAsync(r => r.codigo == codigo, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }
 public async Task<Rol> CreateAsync(Rol nuevo, CancellationToken cancellationToken = default)
 {
 nuevo.Id ??= ObjectId.GenerateNewId().ToString();
 await _collection.InsertOneAsync(nuevo, cancellationToken: cancellationToken);
 return nuevo;
 }
 public async Task UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
 {
 await _collection.ReplaceOneAsync(r => r.Id == rol.Id, rol, cancellationToken: cancellationToken);
 }
 public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
 {
 await _collection.DeleteOneAsync(r => r.Id == id, cancellationToken);
 }
}
