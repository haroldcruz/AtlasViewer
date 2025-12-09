using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models
{
 public class Rol
 {
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; } // maps _id ObjectId -> string
 [BsonElement("id_rol")]
 public int id_rol { get; set; }
 [BsonElement("nombre_rol")]
 public string nombre_rol { get; set; } = string.Empty;
 }
}
