namespace Fabrica.Models.ViewModels.Admin;

public class UserListItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
}
