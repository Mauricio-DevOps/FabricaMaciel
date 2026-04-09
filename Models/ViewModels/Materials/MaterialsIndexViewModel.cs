namespace Fabrica.Models.ViewModels.Materials;

public class MaterialsIndexViewModel
{
    public required IEnumerable<AccessoryListItemViewModel> Acessorios { get; init; }
    public required IEnumerable<DiskListItemViewModel> Discos { get; init; }
    public string SelectedTab { get; init; } = "acessorios";
}
