namespace Fabrica.Models.ViewModels.Items;

public class ItemListItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? Numero { get; set; }
    public string DiscoPrincipal { get; set; } = string.Empty;
    public bool PossuiTampa { get; set; }
    public string? DiscoTampa { get; set; }
    public string AcessoriosResumo { get; set; } = "-";

    public string NomeDisplay => Numero.HasValue ? $"{Nome} {Numero}" : Nome;
}
