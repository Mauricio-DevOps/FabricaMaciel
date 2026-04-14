namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidoItemCatalogOptionViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal? PrecoPromocional { get; set; }
    public decimal? PrecoAtacado { get; set; }
    public decimal? PrecoVarejo { get; set; }
}
