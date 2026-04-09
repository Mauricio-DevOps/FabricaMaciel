namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidoClienteOptionViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
