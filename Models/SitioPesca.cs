using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class SitioPesca
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(50)]
 public string codigo { get; set; } = string.Empty;

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [StringLength(500)]
 public string descripcion { get; set; } = string.Empty;

 public bool activo { get; set; } = true;
}
