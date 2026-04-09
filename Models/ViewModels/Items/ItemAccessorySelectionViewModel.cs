using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models.ViewModels.Items;

public class ItemAccessorySelectionViewModel
{
    public int AcessorioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PesoUnitarioGramas { get; set; }
    public bool Selecionado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; } = 1;
}
