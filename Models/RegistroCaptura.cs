using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class RegistroCaptura
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? pescadorId { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? embarcacionId { get; set; }

 [Required]
 public DateTime fecha { get; set; } = DateTime.UtcNow;

 [Required]
 public string hora { get; set; } = string.Empty;

 [BsonRepresentation(BsonType.ObjectId)]
 public string? sitioPescaId { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? artePescaId { get; set; }

 public List<CapturaDetalle> capturas { get; set; } = new();
 
 // Insumos a nivel de registro (jornada completa)
 public List<AlistoInsumo> insumos { get; set; } = new();
}
