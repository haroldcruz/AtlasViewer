using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class DetalleCompraInsumo
{
 [BsonRepresentation(BsonType.ObjectId)]
 public string? insumoId { get; set; }

 [Range(0, double.MaxValue)]
 public double cantidad { get; set; }

 [Range(0, double.MaxValue)]
 public decimal costoUnitario { get; set; }

 [Range(0, double.MaxValue)]
 public decimal subtotal { get; set; }
}

public class CompraInsumo
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required]
 public DateTime fecha { get; set; } = DateTime.UtcNow;

 [BsonRepresentation(BsonType.ObjectId)]
 public string? proveedorId { get; set; }

 public List<DetalleCompraInsumo> detalle { get; set; } = new();

 [Range(0, double.MaxValue)]
 public decimal total { get; set; }
}
