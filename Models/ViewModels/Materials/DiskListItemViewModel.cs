namespace Fabrica.Models.ViewModels.Materials;

public class DiskListItemViewModel
{
    public int Id { get; set; }
    public int RaioMm { get; set; }
    public decimal GrossuraMm { get; set; }
    public decimal PesoUnitarioKg { get; set; }

    public string MedidaDisplay => $"{RaioMm} x {GrossuraMm:0.##}";
}
