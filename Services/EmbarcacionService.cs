using AtlasViewer.Models;
using MongoDB.Driver;

namespace AtlasViewer.Services;

public interface IEmbarcacionService
{
    Task<List<Embarcacion>> GetAllAsync();
    Task<Embarcacion?> GetByIdAsync(string id);
    Task<List<Embarcacion>> GetByPescadorAsync(string pescadorId);
    Task CreateAsync(Embarcacion embarcacion);
    Task UpdateAsync(string id, Embarcacion embarcacion);
    Task<bool> DeleteAsync(string id);
}

public class EmbarcacionService : IEmbarcacionService
{
    private readonly IMongoCollection<Embarcacion> _embarcaciones;
    private readonly IMongoCollection<Pescador> _pescadores;

    public EmbarcacionService(MongoService mongoService)
    {
        _embarcaciones = mongoService.Database.GetCollection<Embarcacion>("embarcaciones");
        _pescadores = mongoService.Database.GetCollection<Pescador>("pescadores");
    }

    public async Task<List<Embarcacion>> GetAllAsync()
    {
        return await _embarcaciones.Find(_ => true).ToListAsync();
    }

    public async Task<Embarcacion?> GetByIdAsync(string id)
    {
        return await _embarcaciones.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Embarcacion>> GetByPescadorAsync(string pescadorId)
    {
        return await _embarcaciones.Find(e => e.pescadorId == pescadorId).ToListAsync();
    }

    public async Task CreateAsync(Embarcacion embarcacion)
    {
        // Convertir cadena vacía a null
        if (string.IsNullOrWhiteSpace(embarcacion.pescadorId))
        {
            embarcacion.pescadorId = null;
        }
        
        await _embarcaciones.InsertOneAsync(embarcacion);
        
        // Actualizar lista de embarcaciones del pescador
        if (!string.IsNullOrEmpty(embarcacion.pescadorId))
        {
            await ActualizarEmbarcacionesPescadorAsync(embarcacion.pescadorId);
        }
    }

    public async Task UpdateAsync(string id, Embarcacion embarcacion)
    {
        var embarcacionAnterior = await GetByIdAsync(id);
        
        // Convertir cadena vacía a null
        if (string.IsNullOrWhiteSpace(embarcacion.pescadorId))
        {
            embarcacion.pescadorId = null;
        }
        
        await _embarcaciones.ReplaceOneAsync(e => e.Id == id, embarcacion);
        
        // Si cambió el pescador, actualizar ambos
        if (embarcacionAnterior?.pescadorId != embarcacion.pescadorId)
        {
            if (!string.IsNullOrEmpty(embarcacionAnterior?.pescadorId))
            {
                await ActualizarEmbarcacionesPescadorAsync(embarcacionAnterior.pescadorId);
            }
            if (!string.IsNullOrEmpty(embarcacion.pescadorId))
            {
                await ActualizarEmbarcacionesPescadorAsync(embarcacion.pescadorId);
            }
        }
        else if (!string.IsNullOrEmpty(embarcacion.pescadorId))
        {
            // Si solo cambió el nombre/matricula, actualizar el pescador actual
            await ActualizarEmbarcacionesPescadorAsync(embarcacion.pescadorId);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var embarcacion = await GetByIdAsync(id);
        if (embarcacion == null) return false;
        
        var result = await _embarcaciones.DeleteOneAsync(e => e.Id == id);
        
        // Actualizar lista de embarcaciones del pescador
        if (!string.IsNullOrEmpty(embarcacion.pescadorId))
        {
            await ActualizarEmbarcacionesPescadorAsync(embarcacion.pescadorId);
        }
        
        return result.DeletedCount > 0;
    }

    private async Task ActualizarEmbarcacionesPescadorAsync(string pescadorId)
    {
        var embarcaciones = await GetByPescadorAsync(pescadorId);
        var nombresEmbarcaciones = embarcaciones
            .Where(e => e.activo)
            .Select(e => $"{e.nombre} ({e.matricula})")
            .ToList();
        
        var update = Builders<Pescador>.Update.Set(p => p.embarcaciones, nombresEmbarcaciones);
        await _pescadores.UpdateOneAsync(p => p.Id == pescadorId, update);
    }
}
