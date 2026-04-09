namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidoListItemViewModel
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string Status { get; set; } = string.Empty;
    public int QuantidadeItens { get; set; }
    public decimal TotalPedido { get; set; }
    public string TotalPedidoDisplay => TotalPedido.ToString("C");
}
