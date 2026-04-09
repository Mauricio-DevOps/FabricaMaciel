using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fabrica.Models.ViewModels.Admin;

public class UserFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Selecione o nível de acesso.")]
    [Display(Name = "Nível de acesso")]
    public int NivelAcessoId { get; set; }

    public IEnumerable<SelectListItem> NiveisAcesso { get; set; } = Enumerable.Empty<SelectListItem>();

    public string Title => Id.HasValue ? "Editar usuário" : "Criar usuário";
    public bool RequiresPassword => !Id.HasValue;
}
