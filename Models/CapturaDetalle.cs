using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class AlistoInsumo
{
 [BsonRepresentation(BsonType.ObjectId)]
 public string? insumoId { get; set; }

 [Range(0, double.MaxValue)]
 public double cantidad { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioUnitario { get; set; }

 [Range(0, double.MaxValue)]
 public decimal subtotal { get; set; }
}

public class CapturaDetalle
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? registroId { get; set; }

 [BsonRepresentation(BsonType.ObjectId)]
 public string? especieId { get; set; }

 [Range(0, int.MaxValue)]
 public int numeroPeces { get; set; }

 [Range(0, double.MaxValue)]
 public double pesoTotalKg { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioKilo { get; set; }

 [Range(0, double.MaxValue)]
 public decimal total { get; set; }

 // Lista principal de insumos
 public List<AlistoInsumo> insumos { get; set; } = new();
 
 // Campo legacy - MongoDB puede leerlo pero lo ignoramos al escribir
 [BsonIgnoreIfDefault]
 public List<AlistoInsumo> alisto { get; set; } = new();

 // Captura cualquier campo extra (como 'alisto' legacy) sin causar error
 [BsonExtraElements]
 public BsonDocument? extraElements { get; set; }
}
