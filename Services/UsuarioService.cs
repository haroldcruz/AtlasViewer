using AtlasViewer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using BCrypt.Net;

namespace AtlasViewer.Services;

public class MongoSettings
{
 public string ConnectionString { get; set; } = string.Empty;
 public string Database { get; set; } = string.Empty;
 public string Collection { get; set; } = "Usuarios";
}

public interface IUsuarioService
{
 Task<IList<Usuario>> GetUsuariosAsync(CancellationToken cancellationToken = default);
 Task<Usuario?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
 Task<Usuario?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default);
 Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
 Task<bool> VerifyPasswordAsync(Usuario usuario, string plainPassword, CancellationToken cancellationToken = default);
 Task<Usuario> CreateAsync(Usuario nuevo, CancellationToken cancellationToken = default);
 Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
 Task DeleteAsync(string id, CancellationToken cancellationToken = default);
 Task<long> CountByRolAsync(int rol, CancellationToken cancellationToken = default);
 Task UpdateRolEnCascadaAsync(int rolAnterior, int rolNuevo, CancellationToken cancellationToken = default);
}

public class UsuarioService : IUsuarioService
{
 private readonly IMongoCollection<Usuario> _collection;
 private readonly IMongoClient _client;

 public UsuarioService(IOptions<MongoSettings> options)
 {
 var cfg = options.Value;
 if (string.IsNullOrWhiteSpace(cfg.ConnectionString))
 throw new ArgumentException("MongoDB ConnectionString no configurado en appsettings.json (clave 'Mongo:ConnectionString')");
 if (string.IsNullOrWhiteSpace(cfg.Database))
 throw new ArgumentException("MongoDB Database no configurado en appsettings.json (clave 'Mongo:Database')");
 if (string.IsNullOrWhiteSpace(cfg.Collection))
 throw new ArgumentException("MongoDB Collection no configurado en appsettings.json (clave 'Mongo:Collection')");

 var settings = MongoClientSettings.FromConnectionString(cfg.ConnectionString);
 settings.ServerApi = new ServerApi(ServerApiVersion.V1);
 // Ajustes para redes lentas y mejorar descubrimiento
 settings.ServerSelectionTimeout = TimeSpan.FromSeconds(120);
 settings.ConnectTimeout = TimeSpan.FromSeconds(60);
 settings.SocketTimeout = TimeSpan.FromSeconds(120);
 settings.LocalThreshold = TimeSpan.FromMilliseconds(50); // ampl�a el umbral de latencia
 settings.RetryWrites = true;
 settings.RetryReads = true;
 settings.ApplicationName = "AtlasViewer";

 if (cfg.ConnectionString.StartsWith("mongodb+srv", StringComparison.OrdinalIgnoreCase))
 {
 settings.DirectConnection = false;
 }
 else
 {
 settings.DirectConnection = true;
 }

 _client = new MongoClient(settings);
 var db = _client.GetDatabase(cfg.Database);
 _collection = db.GetCollection<Usuario>(cfg.Collection);
 }

 private async Task<bool> TryPingAsync(CancellationToken cancellationToken)
 {
 try
 {
 using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
 cts.CancelAfter(TimeSpan.FromSeconds(5));
 var admin = _client.GetDatabase("admin");
 var cmd = new BsonDocument("ping",1);
 await admin.RunCommandAsync<BsonDocument>(cmd, cancellationToken: cts.Token);
 return true;
 }
 catch (OperationCanceledException)
 {
 // Ping cancelado por timeout breve: ignorar y permitir que la operaci�n principal lo intente
 return false;
 }
 catch (MongoException)
 {
 // Problemas de red/cluster: ignorar aqu�, la operaci�n principal podr� reflejar el error
 return false;
 }
 }

 public async Task<IList<Usuario>> GetUsuariosAsync(CancellationToken cancellationToken = default)
 {
 // Intento de ping opcional sin bloquear
 _ = TryPingAsync(cancellationToken);
 var cursor = await _collection.FindAsync(FilterDefinition<Usuario>.Empty, cancellationToken: cancellationToken);
 return await cursor.ToListAsync(cancellationToken);
 }

 public async Task<Usuario?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 var cursor = await _collection.FindAsync(u => u.Id == id, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }

 public async Task<Usuario?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 var cursor = await _collection.FindAsync(u => u.nombre == nombre, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }

 public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 var cursor = await _collection.FindAsync(u => u.email == email, cancellationToken: cancellationToken);
 return await cursor.FirstOrDefaultAsync(cancellationToken);
 }

 public Task<bool> VerifyPasswordAsync(Usuario usuario, string plainPassword, CancellationToken cancellationToken = default)
 {
 // BCrypt verification using stored hash. No DB operations here.
 if (usuario is null) return Task.FromResult(false);
 var hash = usuario.hash;
 if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(plainPassword)) return Task.FromResult(false);
 var ok = BCrypt.Net.BCrypt.Verify(plainPassword, hash);
 return Task.FromResult(ok);
 }

 public async Task<Usuario> CreateAsync(Usuario nuevo, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 nuevo.Id ??= ObjectId.GenerateNewId().ToString();
 await _collection.InsertOneAsync(nuevo, cancellationToken: cancellationToken);
 return nuevo;
 }

 public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 await _collection.ReplaceOneAsync(u => u.Id == usuario.Id, usuario, cancellationToken: cancellationToken);
 }

 public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 await _collection.DeleteOneAsync(u => u.Id == id, cancellationToken);
 }

 public async Task<long> CountByRolAsync(int rol, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 return await _collection.CountDocumentsAsync(u => u.rol == rol, cancellationToken: cancellationToken);
 }

 public async Task UpdateRolEnCascadaAsync(int rolAnterior, int rolNuevo, CancellationToken cancellationToken = default)
 {
 _ = TryPingAsync(cancellationToken);
 var filter = Builders<Usuario>.Filter.Eq(u => u.rol, rolAnterior);
 var update = Builders<Usuario>.Update.Set(u => u.rol, rolNuevo);
 await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
 }
}