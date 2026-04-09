using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidosIndexViewModel
{
    public List<PedidoListItemViewModel> Pedidos { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public int QuantidadeEmNegociacao { get; set; }
    public int QuantidadeEmProducao { get; set; }
    public int QuantidadeFinalizados { get; set; }
    public int QuantidadeEntregues { get; set; }
}
