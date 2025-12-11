using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Proveedor
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [StringLength(50)]
 public string telefono { get; set; } = string.Empty;

 [EmailAddress, StringLength(200)]
 public string correo { get; set; } = string.Empty;

 public bool activo { get; set; } = true;
}
