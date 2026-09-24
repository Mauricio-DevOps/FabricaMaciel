using System.Globalization;
using Fabrica.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Data;

public static class SqliteDataImporter
{
    private static readonly string[] Tables =
    [
        "NivelAcesso", "Usuario", "Acessorio", "Disco", "Item", "ItemAcessorio",
        "Cliente", "Pedido", "PedidoItem", "EstoqueMovimento"
    ];

    public static async Task VerifyAsync(string sqlitePath, TextWriter output)
    {
        var inventory = await ReadInventoryAsync(sqlitePath);
        await output.WriteLineAsync($"Banco SQLite validado: {Path.GetFullPath(sqlitePath)}");
        await output.WriteLineAsync($"Medida dos discos: {(inventory.UsesRadius ? "raio (será convertido em diâmetro)" : "diâmetro")}");

        foreach (var table in Tables)
        {
            await output.WriteLineAsync($"{table}: {inventory.Counts[table]} registro(s)");
        }
    }

    public static async Task ImportAsync(AppDbContext context, string sqlitePath, TextWriter output)
    {
        var inventory = await ReadInventoryAsync(sqlitePath);
        if (inventory.Counts.Values.Sum() == 0)
        {
            throw new InvalidOperationException("O banco SQLite de origem está vazio. A importação foi cancelada.");
        }

        if (await TargetHasDataAsync(context))
        {
            throw new InvalidOperationException("O banco Azure SQL de destino já contém dados. A importação foi cancelada para evitar duplicação.");
        }

        var executionStrategy = context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var source = new SqliteConnection($"Data Source={Path.GetFullPath(sqlitePath)};Mode=ReadOnly");
            await source.OpenAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            await ImportAsync(context, await ReadAsync(source, "SELECT Id, Nome, Descricao FROM NivelAcesso", reader => new NivelAcesso
            {
                Id = GetInt(reader, 0), Nome = GetString(reader, 1), Descricao = GetNullableString(reader, 2)
            }));

            await ImportAsync(context, await ReadAsync(source, "SELECT Id, Nome, Email, Senha, NivelAcessoId FROM Usuario", reader => new Usuario
        {
            Id = GetInt(reader, 0), Nome = GetString(reader, 1), Email = GetString(reader, 2), Senha = GetString(reader, 3), NivelAcessoId = GetInt(reader, 4)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, Nome, Descricao, PesoUnitarioKg FROM Acessorio", reader => new Acessorio
        {
            Id = GetInt(reader, 0), Nome = GetString(reader, 1), Descricao = GetNullableString(reader, 2), PesoUnitarioKg = GetDecimal(reader, 3)
        }));

        var diameterColumn = inventory.UsesRadius ? "RaioMm * 2" : "DiametroMm";
        await ImportAsync(context, await ReadAsync(source, $"SELECT Id, {diameterColumn} AS DiametroMm, GrossuraMm, PesoUnitarioKg FROM Disco", reader => new Disco
        {
            Id = GetInt(reader, 0), DiametroMm = GetInt(reader, 1), GrossuraMm = GetDecimal(reader, 2), PesoUnitarioKg = GetDecimal(reader, 3)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, Nome, Numero, DiscoId, PossuiTampa, DiscoTampaId, PrecoPromocional, PrecoAtacado, PrecoVarejo FROM Item", reader => new Item
        {
            Id = GetInt(reader, 0), Nome = GetString(reader, 1), Numero = GetNullableInt(reader, 2), DiscoId = GetInt(reader, 3),
            PossuiTampa = GetBool(reader, 4), DiscoTampaId = GetNullableInt(reader, 5), PrecoPromocional = GetNullableDecimal(reader, 6),
            PrecoAtacado = GetNullableDecimal(reader, 7), PrecoVarejo = GetNullableDecimal(reader, 8)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, ItemId, AcessorioId, Quantidade FROM ItemAcessorio", reader => new ItemAcessorio
        {
            Id = GetInt(reader, 0), ItemId = GetInt(reader, 1), AcessorioId = GetInt(reader, 2), Quantidade = GetInt(reader, 3)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, Nome, Endereco, Telefone, Email, TabelaPreco FROM Cliente", reader => new Cliente
        {
            Id = GetInt(reader, 0), Nome = GetString(reader, 1), Endereco = GetString(reader, 2), Telefone = GetString(reader, 3),
            Email = GetNullableString(reader, 4), TabelaPreco = GetString(reader, 5)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, ClienteId, DataPedido, Status FROM Pedido", reader => new Pedido
        {
            Id = GetInt(reader, 0), ClienteId = GetInt(reader, 1), DataPedido = GetDateTime(reader, 2), Status = GetString(reader, 3)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, PedidoId, ItemId, Quantidade, ValorUnitario FROM PedidoItem", reader => new PedidoItem
        {
            Id = GetInt(reader, 0), PedidoId = GetInt(reader, 1), ItemId = GetInt(reader, 2), Quantidade = GetInt(reader, 3), ValorUnitario = GetDecimal(reader, 4)
        }));

        await ImportAsync(context, await ReadAsync(source, "SELECT Id, Tipo, Operacao, AcessorioId, DiscoId, ItemId, Quantidade, ConsumoAutomatico, Observacao, DataCriacaoUtc FROM EstoqueMovimento", reader => new EstoqueMovimento
        {
            Id = GetInt(reader, 0), Tipo = GetString(reader, 1), Operacao = GetString(reader, 2), AcessorioId = GetNullableInt(reader, 3),
            DiscoId = GetNullableInt(reader, 4), ItemId = GetNullableInt(reader, 5), Quantidade = GetDecimal(reader, 6),
            ConsumoAutomatico = GetBool(reader, 7), Observacao = GetNullableString(reader, 8), DataCriacaoUtc = GetDateTime(reader, 9)
        }));

            await ValidateTargetCountsAsync(context, inventory.Counts);
            await transaction.CommitAsync();
        });
        await output.WriteLineAsync("Importação concluída e validada com sucesso.");
    }

    private static async Task ImportAsync<T>(AppDbContext context, List<T> entities) where T : class
    {
        if (entities.Count == 0)
        {
            return;
        }

        var tableName = GetTableName<T>();
        await SetIdentityInsertAsync(context, tableName, enabled: true);
        try
        {
            context.AddRange(entities);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
        finally
        {
            await SetIdentityInsertAsync(context, tableName, enabled: false);
        }
    }

    private static Task SetIdentityInsertAsync(AppDbContext context, string tableName, bool enabled)
    {
        var command = (tableName, enabled) switch
        {
            ("NivelAcesso", true) => "SET IDENTITY_INSERT [NivelAcesso] ON",
            ("NivelAcesso", false) => "SET IDENTITY_INSERT [NivelAcesso] OFF",
            ("Usuario", true) => "SET IDENTITY_INSERT [Usuario] ON",
            ("Usuario", false) => "SET IDENTITY_INSERT [Usuario] OFF",
            ("Acessorio", true) => "SET IDENTITY_INSERT [Acessorio] ON",
            ("Acessorio", false) => "SET IDENTITY_INSERT [Acessorio] OFF",
            ("Disco", true) => "SET IDENTITY_INSERT [Disco] ON",
            ("Disco", false) => "SET IDENTITY_INSERT [Disco] OFF",
            ("Item", true) => "SET IDENTITY_INSERT [Item] ON",
            ("Item", false) => "SET IDENTITY_INSERT [Item] OFF",
            ("ItemAcessorio", true) => "SET IDENTITY_INSERT [ItemAcessorio] ON",
            ("ItemAcessorio", false) => "SET IDENTITY_INSERT [ItemAcessorio] OFF",
            ("Cliente", true) => "SET IDENTITY_INSERT [Cliente] ON",
            ("Cliente", false) => "SET IDENTITY_INSERT [Cliente] OFF",
            ("Pedido", true) => "SET IDENTITY_INSERT [Pedido] ON",
            ("Pedido", false) => "SET IDENTITY_INSERT [Pedido] OFF",
            ("PedidoItem", true) => "SET IDENTITY_INSERT [PedidoItem] ON",
            ("PedidoItem", false) => "SET IDENTITY_INSERT [PedidoItem] OFF",
            ("EstoqueMovimento", true) => "SET IDENTITY_INSERT [EstoqueMovimento] ON",
            ("EstoqueMovimento", false) => "SET IDENTITY_INSERT [EstoqueMovimento] OFF",
            _ => throw new InvalidOperationException($"Tabela não suportada para importação: {tableName}.")
        };

        return context.Database.ExecuteSqlRawAsync(command);
    }

    private static string GetTableName<T>() where T : class => typeof(T).Name switch
    {
        nameof(NivelAcesso) => "NivelAcesso",
        nameof(Usuario) => "Usuario",
        nameof(Acessorio) => "Acessorio",
        nameof(Disco) => "Disco",
        nameof(Item) => "Item",
        nameof(ItemAcessorio) => "ItemAcessorio",
        nameof(Cliente) => "Cliente",
        nameof(Pedido) => "Pedido",
        nameof(PedidoItem) => "PedidoItem",
        nameof(EstoqueMovimento) => "EstoqueMovimento",
        _ => throw new InvalidOperationException($"Tabela não suportada para importação: {typeof(T).Name}.")
    };

    private static async Task<SqliteInventory> ReadInventoryAsync(string sqlitePath)
    {
        if (!File.Exists(sqlitePath))
        {
            throw new FileNotFoundException("O arquivo SQLite informado não foi encontrado.", sqlitePath);
        }

        await using var connection = new SqliteConnection($"Data Source={Path.GetFullPath(sqlitePath)};Mode=ReadOnly");
        await connection.OpenAsync();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var table in Tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name";
            command.Parameters.AddWithValue("$name", table);
            if (Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture) != 1)
            {
                throw new InvalidOperationException($"A tabela obrigatória '{table}' não existe no banco SQLite de origem.");
            }

            command.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
            command.Parameters.Clear();
            counts[table] = Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
        }

        var diskColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(\"Disco\")";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                diskColumns.Add(GetString(reader, 1));
            }
        }

        var usesRadius = diskColumns.Contains("RaioMm");
        if (!usesRadius && !diskColumns.Contains("DiametroMm"))
        {
            throw new InvalidOperationException("O banco SQLite não possui RaioMm nem DiametroMm na tabela Disco.");
        }

        return new SqliteInventory(counts, usesRadius);
    }

    private static async Task<List<T>> ReadAsync<T>(SqliteConnection connection, string sql, Func<SqliteDataReader, T> map)
    {
        var entities = new List<T>();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entities.Add(map(reader));
        }

        return entities;
    }

    private static async Task<bool> TargetHasDataAsync(AppDbContext context) =>
        await context.NiveisAcesso.AnyAsync() || await context.Usuarios.AnyAsync() || await context.Acessorios.AnyAsync() ||
        await context.Discos.AnyAsync() || await context.Itens.AnyAsync() || await context.Clientes.AnyAsync() || await context.Pedidos.AnyAsync();

    private static async Task ValidateTargetCountsAsync(AppDbContext context, IReadOnlyDictionary<string, int> counts)
    {
        var targetCounts = new Dictionary<string, int>
        {
            ["NivelAcesso"] = await context.NiveisAcesso.CountAsync(), ["Usuario"] = await context.Usuarios.CountAsync(),
            ["Acessorio"] = await context.Acessorios.CountAsync(), ["Disco"] = await context.Discos.CountAsync(),
            ["Item"] = await context.Itens.CountAsync(), ["ItemAcessorio"] = await context.ItemAcessorios.CountAsync(),
            ["Cliente"] = await context.Clientes.CountAsync(), ["Pedido"] = await context.Pedidos.CountAsync(),
            ["PedidoItem"] = await context.PedidoItens.CountAsync(), ["EstoqueMovimento"] = await context.EstoqueMovimentos.CountAsync()
        };

        foreach (var table in Tables)
        {
            if (targetCounts[table] != counts[table])
            {
                throw new InvalidOperationException($"Validação falhou para {table}: origem {counts[table]}, destino {targetCounts[table]}.");
            }
        }
    }

    private static int GetInt(SqliteDataReader reader, int ordinal) => Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static int? GetNullableInt(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : GetInt(reader, ordinal);
    private static bool GetBool(SqliteDataReader reader, int ordinal) => Convert.ToBoolean(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static decimal GetDecimal(SqliteDataReader reader, int ordinal) => Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    private static decimal? GetNullableDecimal(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : GetDecimal(reader, ordinal);
    private static string GetString(SqliteDataReader reader, int ordinal) => Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
    private static string? GetNullableString(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : GetString(reader, ordinal);
    private static DateTime GetDateTime(SqliteDataReader reader, int ordinal) => Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

    private sealed record SqliteInventory(IReadOnlyDictionary<string, int> Counts, bool UsesRadius);
}
