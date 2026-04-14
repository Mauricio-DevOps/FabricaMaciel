using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Item
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Numero")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um numero valido.")]
    public int? Numero { get; set; }

    [Display(Name = "Disco principal")]
    public int DiscoId { get; set; }

    public Disco Disco { get; set; } = null!;

    [Display(Name = "Possui tampa")]
    public bool PossuiTampa { get; set; }

    [Display(Name = "Disco da tampa")]
    public int? DiscoTampaId { get; set; }

    public Disco? DiscoTampa { get; set; }

    [Display(Name = "Preco promocional")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco promocional valido.")]
    public decimal? PrecoPromocional { get; set; }

    [Display(Name = "Preco atacado")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco atacado valido.")]
    public decimal? PrecoAtacado { get; set; }

    [Display(Name = "Preco varejo")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Informe um preco varejo valido.")]
    public decimal? PrecoVarejo { get; set; }

    public ICollection<ItemAcessorio> ItemAcessorios { get; set; } = new List<ItemAcessorio>();
}
