using Fabrica.Models;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Data;

public static class DatabaseInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Migrate(context);
        SeedNivelAcesso(context);
        SeedAdminUser(context);
    }

    public static void Migrate(AppDbContext context)
    {
        context.Database.Migrate();
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
