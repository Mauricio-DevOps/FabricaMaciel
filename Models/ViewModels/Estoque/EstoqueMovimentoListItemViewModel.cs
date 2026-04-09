namespace Fabrica.Models.ViewModels.Estoque;

public class EstoqueMovimentoListItemViewModel
{
    public string Tipo { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Operacao { get; set; } = string.Empty;
    public string QuantidadeDisplay { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime DataCriacaoLocal { get; set; }
}
