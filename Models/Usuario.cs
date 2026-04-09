using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public int NivelAcessoId { get; set; }

    public NivelAcesso? NivelAcesso { get; set; }
}
