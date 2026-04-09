using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models.ViewModels.Materials;

public class AccessoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    [Display(Name = "Peso por unidade (g)")]
    [Range(typeof(decimal), "0,01", "999999", ErrorMessage = "Informe um peso válido em gramas.")]
    public decimal PesoUnitarioGramas { get; set; }

    public string Title => Id.HasValue ? "Editar acessório" : "Novo acessório";
}
