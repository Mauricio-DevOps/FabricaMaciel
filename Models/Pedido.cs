using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Pedido
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    [Display(Name = "Data do pedido")]
    [DataType(DataType.Date)]
    public DateTime DataPedido { get; set; } = DateTime.Today;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = PedidoStatus.EmNegociacao;

    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
}
