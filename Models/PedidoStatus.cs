namespace Fabrica.Models;

public static class PedidoStatus
{
    public const string EmNegociacao = "Em negociacao";
    public const string EmProducao = "Em producao";
    public const string Finalizado = "Finalizado";
    public const string Entregue = "Entregue";

    public static IReadOnlyList<string> Todos { get; } =
    [
        EmNegociacao,
        EmProducao,
        Finalizado,
        Entregue
    ];

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && Todos.Contains(status, StringComparer.Ordinal);
    }
}
