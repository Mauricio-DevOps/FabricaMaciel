using Fabrica.Models;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Data;

public static class DatabaseInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.Migrate();
        EnsureEstoqueStructure(context);
        SeedNivelAcesso(context);
        SeedAdminUser(context);
    }

    private static void EnsureEstoqueStructure(AppDbContext context)
    {
        context.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "EstoqueMovimento" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_EstoqueMovimento" PRIMARY KEY AUTOINCREMENT,
                "Tipo" TEXT NOT NULL,
                "Operacao" TEXT NOT NULL,
                "AcessorioId" INTEGER NULL,
                "DiscoId" INTEGER NULL,
                "ItemId" INTEGER NULL,
                "Quantidade" TEXT NOT NULL,
                "ConsumoAutomatico" INTEGER NOT NULL,
                "Observacao" TEXT NULL,
                "DataCriacaoUtc" TEXT NOT NULL,
                CONSTRAINT "FK_EstoqueMovimento_Acessorio_AcessorioId" FOREIGN KEY ("AcessorioId") REFERENCES "Acessorio" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_EstoqueMovimento_Disco_DiscoId" FOREIGN KEY ("DiscoId") REFERENCES "Disco" ("Id") ON DELETE RESTRICT,
                CONSTRAINT "FK_EstoqueMovimento_Item_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Item" ("Id") ON DELETE RESTRICT
            );
            """);

        context.Database.ExecuteSqlRaw("""CREATE INDEX IF NOT EXISTS "IX_EstoqueMovimento_AcessorioId" ON "EstoqueMovimento" ("AcessorioId");""");
        context.Database.ExecuteSqlRaw("""CREATE INDEX IF NOT EXISTS "IX_EstoqueMovimento_DiscoId" ON "EstoqueMovimento" ("DiscoId");""");
        context.Database.ExecuteSqlRaw("""CREATE INDEX IF NOT EXISTS "IX_EstoqueMovimento_ItemId" ON "EstoqueMovimento" ("ItemId");""");
        context.Database.ExecuteSqlRaw("""CREATE INDEX IF NOT EXISTS "IX_EstoqueMovimento_DataCriacaoUtc" ON "EstoqueMovimento" ("DataCriacaoUtc");""");

        context.Database.ExecuteSqlRaw(
            """
            INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260407234500_AddEstoqueMovimentos', '9.0.0');
            """);
    }

    private static void SeedNivelAcesso(AppDbContext context)
    {
        var hasChanges = false;

        if (!context.NiveisAcesso.Any(n => n.Id == 1))
        {
            context.NiveisAcesso.Add(new NivelAcesso
            {
                Id = 1,
                Nome = "Admin",
                Descricao = "Todos as funções de Administração"
            });
            hasChanges = true;
        }

        if (!context.NiveisAcesso.Any(n => n.Id == 2))
        {
            context.NiveisAcesso.Add(new NivelAcesso
            {
                Id = 2,
                Nome = "Usuário",
                Descricao = "Funcionalidades padrão do sistema"
            });
            hasChanges = true;
        }

        if (hasChanges)
        {
            context.SaveChanges();
        }
    }

    private static void SeedAdminUser(AppDbContext context)
    {
        if (context.Usuarios.Any(u => u.Email == "admin@gmail.com"))
        {
            return;
        }

        context.Usuarios.Add(new Usuario
        {
            Nome = "admin",
            Email = "admin@gmail.com",
            Senha = "741852963",
            NivelAcessoId = 1
        });
        context.SaveChanges();
    }
}
