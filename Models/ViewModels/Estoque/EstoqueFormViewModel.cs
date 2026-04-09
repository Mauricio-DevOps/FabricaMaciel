using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Estoque;

public class EstoqueFormViewModel
{
    public const string TipoAcessorio = "acessorio";
    public const string TipoDisco = "disco";
    public const string TipoItem = "item";

    public const string OperacaoEntrada = "entrada";
    public const string OperacaoSaida = "saida";

    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TipoAcessorio;

    [Display(Name = "Operacao")]
    public string Operacao { get; set; } = OperacaoEntrada;

    [Display(Name = "Acessorio")]
    public int? AcessorioId { get; set; }

    [Display(Name = "Disco")]
    public int? DiscoId { get; set; }

    [Display(Name = "Equipamento / Item")]
    public int? ItemId { get; set; }

    [Display(Name = "Quantidade (kg)")]
    [Range(typeof(decimal), "0,0001", "999999999", ErrorMessage = "Informe uma quantidade valida em kg.")]
    public decimal? QuantidadeKg { get; set; }

    [Display(Name = "Quantidade (unidades)")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma quantidade valida em unidades.")]
    public int? QuantidadeUnidades { get; set; }

    public bool ConfirmarSemConsumo { get; set; }
    public bool MostrarConfirmacaoSemConsumo { get; set; }
    public List<string> AvisosSemEstoque { get; set; } = new();

    public List<SelectListItem> TipoOptions { get; set; } = new();
    public List<SelectListItem> OperacaoOptions { get; set; } = new();
    public List<SelectListItem> AcessorioOptions { get; set; } = new();
    public List<SelectListItem> DiscoOptions { get; set; } = new();
    public List<SelectListItem> ItemOptions { get; set; } = new();
}
