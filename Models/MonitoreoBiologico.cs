using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class MonitoreoBiologico
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? registroId { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? especieId { get; set; }

 [Range(0, double.MaxValue)]
 public double tallaCm { get; set; }

 [Range(0, double.MaxValue)]
 public double pesoKg { get; set; }

 public bool eviscerado { get; set; }

 [StringLength(50)]
 public string sexo { get; set; } = string.Empty;

 [StringLength(100)]
 public string madurez { get; set; } = string.Empty;

 [StringLength(500)]
 public string observaciones { get; set; } = string.Empty;
}
