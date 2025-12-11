using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Pescador
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [Required, StringLength(50)]
 public string identificacion { get; set; } = string.Empty;

 public List<string> embarcaciones { get; set; } = new();

 public bool activo { get; set; } = true;
}
