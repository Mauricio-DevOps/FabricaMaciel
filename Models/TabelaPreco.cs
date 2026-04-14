namespace Fabrica.Models;

public static class TabelaPreco
{
    public const string Promocional = "Promocional";
    public const string Atacado = "Atacado";
    public const string Varejo = "Varejo";

    public static IReadOnlyList<string> Todas { get; } =
    [
        Promocional,
        Atacado,
        Varejo
    ];

    public static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Todas.Contains(value);
    }

    public static decimal? ObterPreco(Item item, string? tabelaPreco)
    {
        return tabelaPreco switch
        {
            Promocional => item.PrecoPromocional,
            Atacado => item.PrecoAtacado,
            Varejo => item.PrecoVarejo,
            _ => null
        };
    }
}
