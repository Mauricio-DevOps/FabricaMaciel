using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models.ViewModels.Pedidos;

public class NovoClienteViewModel
{
    [Required(ErrorMessage = "Informe o nome do cliente.")]
    [Display(Name = "Nome do cliente")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereco do cliente.")]
    [Display(Name = "Endereco")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone do cliente.")]
    [Display(Name = "Telefone")]
    public string Telefone { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Selecione a tabela de preco do cliente.")]
    [Display(Name = "Tabela de preco")]
    public string TabelaPreco { get; set; } = Fabrica.Models.TabelaPreco.Varejo;
}
