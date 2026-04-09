using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class NivelAcesso
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descricao { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
