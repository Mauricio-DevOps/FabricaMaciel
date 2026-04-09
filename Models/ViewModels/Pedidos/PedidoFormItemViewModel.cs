using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidoFormItemViewModel
{
    [Display(Name = "Item")]
    public int? ItemId { get; set; }

    [Display(Name = "Quantidade")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma quantidade valida.")]
    public int Quantidade { get; set; } = 1;

    [Display(Name = "Valor unitario")]
    [Range(typeof(decimal), "0,01", "999999999", ErrorMessage = "Informe um valor unitario valido.")]
    public decimal? ValorUnitario { get; set; }

    public decimal ValorTotal => Quantidade > 0 && ValorUnitario.HasValue
        ? Quantidade * ValorUnitario.Value
        : 0;
}
