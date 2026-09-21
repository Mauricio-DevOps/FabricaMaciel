using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models.ViewModels.Materials;

public class DiskFormViewModel
{
    public const decimal AluminumDensityGramsPerCm3 = 2.12m;

    public int? Id { get; set; }

    [Display(Name = "Diâmetro (mm)")]
    [Range(1, int.MaxValue, ErrorMessage = "O diâmetro deve ser maior que zero.")]
    public int DiametroMm { get; set; }

    [Display(Name = "Grossura (mm)")]
    [Range(typeof(decimal), "0,0001", "999999", ErrorMessage = "A grossura deve ser maior que zero.")]
    public decimal GrossuraMm { get; set; }

    [Display(Name = "Peso unitário (kg)")]
    [Range(typeof(decimal), "0,0001", "999999", ErrorMessage = "O peso deve ser maior que zero.")]
    public decimal PesoUnitarioKg { get; set; }

    [Display(Name = "Calcular peso automaticamente para alumínio")]
    public bool CalcularPesoAutomaticamente { get; set; } = true;

    public decimal PesoUnitarioGramasEstimado => Math.Round(CalculateAluminumWeightGrams(DiametroMm, GrossuraMm), 2);
    public decimal PesoUnitarioKgEstimado => Math.Round(PesoUnitarioGramasEstimado / 1000m, 4);

    public string Title => Id.HasValue ? "Editar disco" : "Novo disco";
    public string MedidaPreview => $"{DiametroMm} x {GrossuraMm:0.##}";

    public static decimal CalculateAluminumWeightGrams(int diametroMm, decimal grossuraMm)
    {
        if (diametroMm <= 0 || grossuraMm <= 0)
        {
            return 0m;
        }

        var radiusCm = diametroMm / 20m;
        var thicknessCm = grossuraMm / 10m;
        var volumeCm3 = (decimal)Math.PI * radiusCm * radiusCm * thicknessCm;
        return volumeCm3 * AluminumDensityGramsPerCm3;
    }
}
