using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Usuario
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(100)]
 public string nombre { get; set; } = string.Empty;

 [Required, EmailAddress, StringLength(200)]
 public string email { get; set; } = string.Empty;

 [Range(0, int.MaxValue)]
 public int rol { get; set; }

 public string? hash { get; set; }

 public bool mustChangePassword { get; set; } = false; // obliga cambio en primer inicio
}