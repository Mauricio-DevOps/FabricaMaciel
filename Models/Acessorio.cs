using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Acessorio
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Informe um peso válido.")]
    [Display(Name = "Peso unitário (kg)")]
    public decimal PesoUnitarioKg { get; set; }
}
