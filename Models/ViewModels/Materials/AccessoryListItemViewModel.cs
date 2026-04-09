namespace Fabrica.Models.ViewModels.Materials;

public class AccessoryListItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PesoUnitarioGramas { get; set; }
}
