using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Items;

public class ItemFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do produto.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Número")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um numero válido.")]
    public int? Numero { get; set; }

    [Display(Name = "Disco principal")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o disco principal.")]
    public int DiscoId { get; set; }

    [Display(Name = "Possui tampa")]
    public bool PossuiTampa { get; set; }

    [Display(Name = "Disco da tampa")]
    public int? DiscoTampaId { get; set; }

    [Display(Name = "Preço promocional")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preço promocional válido.")]
    public decimal? PrecoPromocional { get; set; }

    [Display(Name = "Preço de atacado")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preço atacado válido.")]
    public decimal? PrecoAtacado { get; set; }

    [Display(Name = "Preço de varejo")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preço varejo válido.")]
    public decimal? PrecoVarejo { get; set; }

    public List<SelectListItem> DiscoOptions { get; set; } = new();
    public List<ItemAccessorySelectionViewModel> Acessorios { get; set; } = new();

    public string Title => Id.HasValue ? "Editar produto" : "Novo produto";
}
