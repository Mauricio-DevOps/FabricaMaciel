using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class EstoqueMovimento
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Operacao { get; set; } = string.Empty;

    public int? AcessorioId { get; set; }
    public Acessorio? Acessorio { get; set; }

    public int? DiscoId { get; set; }
    public Disco? Disco { get; set; }

    public int? ItemId { get; set; }
    public Item? Item { get; set; }

    [Range(typeof(decimal), "0,0001", "999999999", ErrorMessage = "Informe uma quantidade valida.")]
    public decimal Quantidade { get; set; }

    public bool ConsumoAutomatico { get; set; }

    [StringLength(300)]
    public string? Observacao { get; set; }

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
}
