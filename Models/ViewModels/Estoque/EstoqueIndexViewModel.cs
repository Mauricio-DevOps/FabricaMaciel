namespace Fabrica.Models.ViewModels.Estoque;

public class EstoqueIndexViewModel
{
    public required EstoqueFormViewModel Form { get; init; }
    public required IReadOnlyList<EstoqueSaldoViewModel> SaldosAcessorios { get; init; }
    public required IReadOnlyList<EstoqueSaldoViewModel> SaldosDiscos { get; init; }
    public required IReadOnlyList<EstoqueSaldoViewModel> SaldosItens { get; init; }
    public required IReadOnlyList<EstoqueMovimentoListItemViewModel> MovimentosRecentes { get; init; }
}
