using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtlasViewer.Models;

public class Especie
{
 [BsonId]
 [BsonRepresentation(BsonType.ObjectId)]
 public string? Id { get; set; }

 [Required, StringLength(50)]
 public string codigo { get; set; } = string.Empty;

 [Required, StringLength(200)]
 public string nombre { get; set; } = string.Empty;

 [Range(0, double.MaxValue)]
 public double tallaMinimaCm { get; set; }

 [Range(0, double.MaxValue)]
 public double pesoMinimoKg { get; set; }

 [Range(0, double.MaxValue)]
 public double inventarioAcumuladoKg { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioPromedioCompra { get; set; }

 [Range(0, double.MaxValue)]
 public decimal precioPromedioVenta { get; set; }

 public bool activo { get; set; } = true;
}
