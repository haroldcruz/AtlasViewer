using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models
{
 public class Item
 {
 [BsonId]
 public ObjectId Id { get; set; }

 [BsonElement("name")]
 public string Name { get; set; } = string.Empty;

 [BsonElement("value")]
 public string? Value { get; set; }
 }
}
