using Fabrica.Models;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<NivelAcesso> NiveisAcesso => Set<NivelAcesso>();
    public DbSet<Acessorio> Acessorios => Set<Acessorio>();
    public DbSet<Disco> Discos => Set<Disco>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<ItemAcessorio> ItemAcessorios => Set<ItemAcessorio>();
    public DbSet<EstoqueMovimento> EstoqueMovimentos => Set<EstoqueMovimento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NivelAcesso>(entity =>
        {
            entity.ToTable("NivelAcesso");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(255);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(160);
            entity.Property(e => e.Senha).IsRequired().HasMaxLength(120);

            entity.HasIndex(e => e.Email).IsUnique();

            entity
                .HasOne(e => e.NivelAcesso)
                .WithMany(n => n.Usuarios)
                .HasForeignKey(e => e.NivelAcessoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Acessorio>(entity =>
        {
            entity.ToTable("Acessorio");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.PesoUnitarioKg).HasPrecision(10, 4);
        });

        modelBuilder.Entity<Disco>(entity =>
        {
            entity.ToTable("Disco");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RaioMm).IsRequired();
            entity.Property(e => e.GrossuraMm).HasPrecision(10, 4);
            entity.Property(e => e.PesoUnitarioKg).HasPrecision(10, 4);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PrecoPromocional).HasPrecision(14, 2);
            entity.Property(e => e.PrecoAtacado).HasPrecision(14, 2);
            entity.Property(e => e.PrecoVarejo).HasPrecision(14, 2);

            entity.HasOne(e => e.Disco)
                .WithMany()
                .HasForeignKey(e => e.DiscoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DiscoTampa)
                .WithMany()
                .HasForeignKey(e => e.DiscoTampaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ItemAcessorio>(entity =>
        {
            entity.ToTable("ItemAcessorio");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantidade).IsRequired();

            entity.HasIndex(e => new { e.ItemId, e.AcessorioId }).IsUnique();

            entity.HasOne(e => e.Item)
                .WithMany(i => i.ItemAcessorios)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Acessorio)
                .WithMany()
                .HasForeignKey(e => e.AcessorioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Endereco).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(40);
            entity.Property(e => e.Email).HasMaxLength(160);
            entity.Property(e => e.TabelaPreco).IsRequired().HasMaxLength(30);

            entity.HasIndex(e => e.Nome);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedido");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DataPedido).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);

            entity.HasIndex(e => e.ClienteId);
            entity.HasIndex(e => e.DataPedido);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.ToTable("PedidoItem");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantidade).IsRequired();
            entity.Property(e => e.ValorUnitario).HasPrecision(14, 2);

            entity.HasIndex(e => e.PedidoId);
            entity.HasIndex(e => new { e.PedidoId, e.ItemId }).IsUnique();

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EstoqueMovimento>(entity =>
        {
            entity.ToTable("EstoqueMovimento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Operacao).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Quantidade).HasPrecision(14, 4);
            entity.Property(e => e.Observacao).HasMaxLength(300);
            entity.Property(e => e.DataCriacaoUtc).IsRequired();

            entity.HasIndex(e => e.DataCriacaoUtc);
            entity.HasIndex(e => e.AcessorioId);
            entity.HasIndex(e => e.DiscoId);
            entity.HasIndex(e => e.ItemId);

            entity.HasOne(e => e.Acessorio)
                .WithMany()
                .HasForeignKey(e => e.AcessorioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Disco)
                .WithMany()
                .HasForeignKey(e => e.DiscoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
