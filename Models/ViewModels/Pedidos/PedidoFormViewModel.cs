using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Pedidos;

public class PedidoFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Cliente")]
    public int? ClienteId { get; set; }

    [Display(Name = "Data do pedido")]
    [DataType(DataType.Date)]
    public DateTime DataPedido { get; set; } = DateTime.Today;

    [Display(Name = "Status")]
    public string Status { get; set; } = PedidoStatus.EmNegociacao;

    public List<PedidoFormItemViewModel> Itens { get; set; } = new();

    [ValidateNever]
    public NovoClienteViewModel NovoCliente { get; set; } = new();

    public List<SelectListItem> ClienteOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public List<SelectListItem> ItemOptions { get; set; } = new();

    public List<PedidoClienteOptionViewModel> ClientesCatalogo { get; set; } = new();
    public List<PedidoItemCatalogOptionViewModel> ItensCatalogo { get; set; } = new();

    public decimal TotalPedido => Itens.Sum(i => i.ValorTotal);

    public string Title => Id.HasValue ? "Editar pedido" : "Novo pedido";
}
