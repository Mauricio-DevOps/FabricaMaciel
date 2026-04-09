using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Disco
{
    public int Id { get; set; }

    [Display(Name = "Raio (mm)")]
    [Range(1, int.MaxValue, ErrorMessage = "O raio deve ser maior que zero.")]
    public int RaioMm { get; set; }

    [Display(Name = "Grossura (mm)")]
    [Range(typeof(decimal), "0,0001", "999999", ErrorMessage = "A grossura deve ser maior que zero.")]
    public decimal GrossuraMm { get; set; }

    [Display(Name = "Peso unitário (kg)")]
    [Range(typeof(decimal), "0,0001", "999999", ErrorMessage = "O peso deve ser maior que zero.")]
    public decimal PesoUnitarioKg { get; set; }
}
