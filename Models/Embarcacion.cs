using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Embarcacion
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [StringLength(50)]
 public string matricula { get; set; } = string.Empty;

    [Required]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? pescadorId { get; set; }

    public bool activo { get; set; } = true;
}
