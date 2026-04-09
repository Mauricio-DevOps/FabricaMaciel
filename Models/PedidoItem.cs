using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class PedidoItem
{
    public int Id { get; set; }

    [Required]
    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;

    [Required]
    [Display(Name = "Item")]
    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Informe uma quantidade valida.")]
    public int Quantidade { get; set; }

    [Display(Name = "Valor unitario")]
    [Range(typeof(decimal), "0,01", "999999999", ErrorMessage = "Informe um valor unitario valido.")]
    public decimal ValorUnitario { get; set; }
}
