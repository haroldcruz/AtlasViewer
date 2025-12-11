using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class PescaIncidental
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? registroId { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? especieId { get; set; }

 [Range(0, int.MaxValue)]
 public int individuos { get; set; }

 [Range(0, double.MaxValue)]
 public double pesoTotalKg { get; set; }
}
