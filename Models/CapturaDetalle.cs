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

 public List<AlistoInsumo> alisto { get; set; } = new();
 public List<AlistoInsumo> insumos { get; set; } = new(); // Alias para compatibilidad
}
