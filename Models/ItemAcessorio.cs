using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class ItemAcessorio
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public int AcessorioId { get; set; }

    public Acessorio Acessorio { get; set; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}
