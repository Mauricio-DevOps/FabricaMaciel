using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do cliente.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereco do cliente.")]
    [StringLength(250)]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone do cliente.")]
    [StringLength(40)]
    public string Telefone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [StringLength(160)]
    public string? Email { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
