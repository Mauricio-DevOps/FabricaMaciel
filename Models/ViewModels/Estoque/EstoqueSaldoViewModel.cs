namespace Fabrica.Models.ViewModels.Estoque;

public class EstoqueSaldoViewModel
{
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public string QuantidadeDisplay { get; set; } = string.Empty;
}
