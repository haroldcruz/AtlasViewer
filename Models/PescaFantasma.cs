using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class PescaFantasma
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? registroId { get; set; }

 [Required, StringLength(200)]
 public string tipoArte { get; set; } = string.Empty;

 [StringLength(500)]
 public string especiesAfectadas { get; set; } = string.Empty;

 [StringLength(500)]
 public string ubicacion { get; set; } = string.Empty;

 public bool liberacion { get; set; }
}
