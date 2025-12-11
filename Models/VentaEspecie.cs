using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class DetalleVentaEspecie
{
 [BsonRepresentation(BsonType.ObjectId)]
 public string? especieId { get; set; }

 [Range(0, double.MaxValue)]
 public double cantidadKg { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioUnitario { get; set; }

 [Range(0, double.MaxValue)]
 public decimal subtotal { get; set; }
}

public class VentaEspecie
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(50)]
 public string consecutivo { get; set; } = string.Empty;

 [Required]
 public DateTime fecha { get; set; } = DateTime.UtcNow;

 [Required, StringLength(200)]
 public string cliente { get; set; } = string.Empty;

 public List<DetalleVentaEspecie> detalle { get; set; } = new();

 [Range(0, double.MaxValue)]
 public decimal total { get; set; }
}
