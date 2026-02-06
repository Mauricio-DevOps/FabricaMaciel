using System.ComponentModel.DataAnnotations;

namespace Fabrica.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Informe o nome de usuário.")]
    [Display(Name = "Nome de usuário")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;
}
