using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Insumo
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(50)]
 public string codigo { get; set; } = string.Empty;

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [Required, StringLength(50)]
 public string unidad { get; set; } = string.Empty;

 [Range(0, double.MaxValue)]
 public double inventarioActual { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioPromedioCompra { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioPromedioVenta { get; set; }

 public bool activo { get; set; } = true;
}
