using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Items;

public class ItemFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do item.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Numero")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um numero valido.")]
    public int? Numero { get; set; }

    [Display(Name = "Disco principal")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o disco principal.")]
    public int DiscoId { get; set; }

    [Display(Name = "Possui tampa")]
    public bool PossuiTampa { get; set; }

    [Display(Name = "Disco da tampa")]
    public int? DiscoTampaId { get; set; }

    [Display(Name = "Preco promocional")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco promocional valido.")]
    public decimal? PrecoPromocional { get; set; }

    [Display(Name = "Preco atacado")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco atacado valido.")]
    public decimal? PrecoAtacado { get; set; }

    [Display(Name = "Preco varejo")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco varejo valido.")]
    public decimal? PrecoVarejo { get; set; }

    public List<SelectListItem> DiscoOptions { get; set; } = new();
    public List<ItemAccessorySelectionViewModel> Acessorios { get; set; } = new();

    public string Title => Id.HasValue ? "Editar item" : "Novo item";
}
