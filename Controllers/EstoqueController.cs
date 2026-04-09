using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Models.ViewModels.Estoque;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class EstoqueController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public EstoqueController(AppDbContext context, ILoginCacheService loginCacheService)
    {
        _context = context;
        _loginCacheService = loginCacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var model = await BuildIndexViewModelAsync(new EstoqueFormViewModel());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Movimentar(EstoqueFormViewModel form)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        await PopulateFormOptionsAsync(form);
        await ValidateFormAsync(form);

        if (!ModelState.IsValid)
        {
            return View("Index", await BuildIndexViewModelAsync(form));
        }

        switch (form.Tipo)
        {
            case EstoqueFormViewModel.TipoAcessorio:
                await RegistrarMovimentoAcessorioAsync(form);
                TempData["StatusMessage"] = "Movimentacao de acessorio registrada com sucesso.";
                break;

            case EstoqueFormViewModel.TipoDisco:
                await RegistrarMovimentoDiscoAsync(form);
                TempData["StatusMessage"] = "Movimentacao de disco registrada com sucesso.";
                break;

            case EstoqueFormViewModel.TipoItem:
                var resultadoItem = await RegistrarMovimentoItemAsync(form);
                if (!resultadoItem.Processado)
                {
                    return View("Index", await BuildIndexViewModelAsync(resultadoItem.FormularioRetorno!));
                }

                TempData["StatusMessage"] = resultadoItem.MensagemSucesso;
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureLoggedAccess()
    {
        if (!_loginCacheService.TryGetLoggedUser(HttpContext.Session.Id, out var user) || user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return null;
    }

    private async Task<EstoqueIndexViewModel> BuildIndexViewModelAsync(EstoqueFormViewModel form)
    {
        await PopulateFormOptionsAsync(form);

        var accessories = await _context.Acessorios.AsNoTracking().OrderBy(a => a.Nome).ToListAsync();
        var disks = (await _context.Discos.AsNoTracking().ToListAsync())
            .OrderBy(d => d.RaioMm)
            .ThenBy(d => d.GrossuraMm)
            .ToList();
        var items = await _context.Itens.AsNoTracking().OrderBy(i => i.Nome).ThenBy(i => i.Numero).ToListAsync();
        var movements = await _context.EstoqueMovimentos
            .AsNoTracking()
            .Include(m => m.Acessorio)
            .Include(m => m.Disco)
            .Include(m => m.Item)
            .OrderByDescending(m => m.DataCriacaoUtc)
            .ToListAsync();

        var saldosAcessorios = accessories
            .Select(a =>
            {
                var saldo = CalculateBalanceKg(movements, EstoqueFormViewModel.TipoAcessorio, a.Id);
                return new EstoqueSaldoViewModel
                {
                    Nome = a.Nome,
                    Quantidade = saldo,
                    Unidade = "kg",
                    QuantidadeDisplay = $"{saldo:0.####} kg"
                };
            })
            .Where(s => s.Quantidade != 0)
            .ToList();

        var saldosDiscos = disks
            .Select(d =>
            {
                var saldo = CalculateBalanceKg(movements, EstoqueFormViewModel.TipoDisco, d.Id);
                return new EstoqueSaldoViewModel
                {
                    Nome = $"{d.RaioMm} x {d.GrossuraMm:0.##} mm",
                    Quantidade = saldo,
                    Unidade = "kg",
                    QuantidadeDisplay = $"{saldo:0.####} kg"
                };
            })
            .Where(s => s.Quantidade != 0)
            .ToList();

        var saldosItens = items
            .Select(i =>
            {
                var saldo = CalculateBalanceUnits(movements, i.Id);
                var nome = i.Numero.HasValue ? $"{i.Nome} {i.Numero}" : i.Nome;
                return new EstoqueSaldoViewModel
                {
                    Nome = nome,
                    Quantidade = saldo,
                    Unidade = "un",
                    QuantidadeDisplay = $"{saldo:0} un"
                };
            })
            .Where(s => s.Quantidade != 0)
            .ToList();

        var movimentosRecentes = movements
            .Take(15)
            .Select(m => new EstoqueMovimentoListItemViewModel
            {
                Tipo = m.Tipo,
                Referencia = ResolveMovementReference(m),
                Operacao = m.Operacao,
                QuantidadeDisplay = m.Tipo == EstoqueFormViewModel.TipoItem
                    ? $"{m.Quantidade:0} un"
                    : $"{m.Quantidade:0.####} kg",
                Origem = m.ConsumoAutomatico ? "Consumo automatico" : "Movimentacao manual",
                DataCriacaoLocal = m.DataCriacaoUtc.ToLocalTime()
            })
            .ToList();

        return new EstoqueIndexViewModel
        {
            Form = form,
            SaldosAcessorios = saldosAcessorios,
            SaldosDiscos = saldosDiscos,
            SaldosItens = saldosItens,
            MovimentosRecentes = movimentosRecentes
        };
    }

    private async Task PopulateFormOptionsAsync(EstoqueFormViewModel form)
    {
        form.TipoOptions =
        [
            new SelectListItem("Acessorio", EstoqueFormViewModel.TipoAcessorio),
            new SelectListItem("Disco", EstoqueFormViewModel.TipoDisco),
            new SelectListItem("Equipamento / Item", EstoqueFormViewModel.TipoItem)
        ];

        form.OperacaoOptions =
        [
            new SelectListItem("Adicionar", EstoqueFormViewModel.OperacaoEntrada),
            new SelectListItem("Remover", EstoqueFormViewModel.OperacaoSaida)
        ];

        var accessories = await _context.Acessorios
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync();
        form.AcessorioOptions = accessories
            .Select(a => new SelectListItem(a.Nome, a.Id.ToString()))
            .ToList();

        var disks = (await _context.Discos
            .AsNoTracking()
            .ToListAsync())
            .OrderBy(d => d.RaioMm)
            .ThenBy(d => d.GrossuraMm)
            .ToList();
        form.DiscoOptions = disks
            .Select(d => new SelectListItem($"{d.RaioMm} x {d.GrossuraMm:0.##} mm - {d.PesoUnitarioKg:0.0000} kg", d.Id.ToString()))
            .ToList();

        var items = await _context.Itens
            .AsNoTracking()
            .OrderBy(i => i.Nome)
            .ThenBy(i => i.Numero)
            .ToListAsync();
        form.ItemOptions = items
            .Select(i =>
            {
                var nome = i.Numero.HasValue ? $"{i.Nome} {i.Numero}" : i.Nome;
                return new SelectListItem(nome, i.Id.ToString());
            })
            .ToList();
    }

    private async Task ValidateFormAsync(EstoqueFormViewModel form)
    {
        if (!new[] { EstoqueFormViewModel.TipoAcessorio, EstoqueFormViewModel.TipoDisco, EstoqueFormViewModel.TipoItem }.Contains(form.Tipo))
        {
            ModelState.AddModelError(nameof(form.Tipo), "Selecione um tipo valido.");
        }

        if (!new[] { EstoqueFormViewModel.OperacaoEntrada, EstoqueFormViewModel.OperacaoSaida }.Contains(form.Operacao))
        {
            ModelState.AddModelError(nameof(form.Operacao), "Selecione uma operacao valida.");
        }

        switch (form.Tipo)
        {
            case EstoqueFormViewModel.TipoAcessorio:
                if (!form.AcessorioId.HasValue || !await _context.Acessorios.AnyAsync(a => a.Id == form.AcessorioId.Value))
                {
                    ModelState.AddModelError(nameof(form.AcessorioId), "Selecione um acessorio valido.");
                }

                if (!form.QuantidadeKg.HasValue || form.QuantidadeKg <= 0)
                {
                    ModelState.AddModelError(nameof(form.QuantidadeKg), "Informe a quantidade em kg.");
                }

                if (form.Operacao == EstoqueFormViewModel.OperacaoSaida && form.AcessorioId.HasValue && form.QuantidadeKg.HasValue)
                {
                    var saldoAtual = await GetCurrentBalanceKgAsync(EstoqueFormViewModel.TipoAcessorio, form.AcessorioId.Value);
                    if (form.QuantidadeKg.Value > saldoAtual)
                    {
                        ModelState.AddModelError(nameof(form.QuantidadeKg), $"Saldo insuficiente. Disponivel: {saldoAtual:0.####} kg.");
                    }
                }
                break;

            case EstoqueFormViewModel.TipoDisco:
                if (!form.DiscoId.HasValue || !await _context.Discos.AnyAsync(d => d.Id == form.DiscoId.Value))
                {
                    ModelState.AddModelError(nameof(form.DiscoId), "Selecione um disco valido.");
                }

                if (!form.QuantidadeKg.HasValue || form.QuantidadeKg <= 0)
                {
                    ModelState.AddModelError(nameof(form.QuantidadeKg), "Informe a quantidade em kg.");
                }

                if (form.Operacao == EstoqueFormViewModel.OperacaoSaida && form.DiscoId.HasValue && form.QuantidadeKg.HasValue)
                {
                    var saldoAtual = await GetCurrentBalanceKgAsync(EstoqueFormViewModel.TipoDisco, form.DiscoId.Value);
                    if (form.QuantidadeKg.Value > saldoAtual)
                    {
                        ModelState.AddModelError(nameof(form.QuantidadeKg), $"Saldo insuficiente. Disponivel: {saldoAtual:0.####} kg.");
                    }
                }
                break;

            case EstoqueFormViewModel.TipoItem:
                if (!form.ItemId.HasValue || !await _context.Itens.AnyAsync(i => i.Id == form.ItemId.Value))
                {
                    ModelState.AddModelError(nameof(form.ItemId), "Selecione um item valido.");
                }

                if (!form.QuantidadeUnidades.HasValue || form.QuantidadeUnidades <= 0)
                {
                    ModelState.AddModelError(nameof(form.QuantidadeUnidades), "Informe a quantidade em unidades.");
                }

                if (form.Operacao == EstoqueFormViewModel.OperacaoSaida && form.ItemId.HasValue && form.QuantidadeUnidades.HasValue)
                {
                    var saldoAtual = await GetCurrentBalanceUnitsAsync(form.ItemId.Value);
                    if (form.QuantidadeUnidades.Value > saldoAtual)
                    {
                        ModelState.AddModelError(nameof(form.QuantidadeUnidades), $"Saldo insuficiente. Disponivel: {saldoAtual:0} un.");
                    }
                }
                break;
        }
    }

    private async Task RegistrarMovimentoAcessorioAsync(EstoqueFormViewModel form)
    {
        _context.EstoqueMovimentos.Add(new EstoqueMovimento
        {
            Tipo = EstoqueFormViewModel.TipoAcessorio,
            Operacao = form.Operacao,
            AcessorioId = form.AcessorioId,
            Quantidade = form.QuantidadeKg!.Value,
            Observacao = "Movimentacao manual"
        });

        await _context.SaveChangesAsync();
    }

    private async Task RegistrarMovimentoDiscoAsync(EstoqueFormViewModel form)
    {
        _context.EstoqueMovimentos.Add(new EstoqueMovimento
        {
            Tipo = EstoqueFormViewModel.TipoDisco,
            Operacao = form.Operacao,
            DiscoId = form.DiscoId,
            Quantidade = form.QuantidadeKg!.Value,
            Observacao = "Movimentacao manual"
        });

        await _context.SaveChangesAsync();
    }

    private async Task<ResultadoMovimentoItem> RegistrarMovimentoItemAsync(EstoqueFormViewModel form)
    {
        var item = await _context.Itens
            .Include(i => i.Disco)
            .Include(i => i.DiscoTampa)
            .Include(i => i.ItemAcessorios)
                .ThenInclude(ia => ia.Acessorio)
            .FirstAsync(i => i.Id == form.ItemId!.Value);

        var quantidadeUnidades = form.QuantidadeUnidades!.Value;

        if (form.Operacao == EstoqueFormViewModel.OperacaoSaida)
        {
            _context.EstoqueMovimentos.Add(new EstoqueMovimento
            {
                Tipo = EstoqueFormViewModel.TipoItem,
                Operacao = form.Operacao,
                ItemId = item.Id,
                Quantidade = quantidadeUnidades,
                Observacao = "Movimentacao manual"
            });

            await _context.SaveChangesAsync();
            return ResultadoMovimentoItem.ProcessadoComSucesso("Movimentacao de item registrada com sucesso.");
        }

        var avisosSemEstoque = await BuildInsufficientStockWarningsAsync(item, quantidadeUnidades);
        if (avisosSemEstoque.Count > 0 && !form.ConfirmarSemConsumo)
        {
            form.MostrarConfirmacaoSemConsumo = true;
            form.AvisosSemEstoque = avisosSemEstoque;
            return ResultadoMovimentoItem.PendenteConfirmacao(form);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        _context.EstoqueMovimentos.Add(new EstoqueMovimento
        {
            Tipo = EstoqueFormViewModel.TipoItem,
            Operacao = EstoqueFormViewModel.OperacaoEntrada,
            ItemId = item.Id,
            Quantidade = quantidadeUnidades,
            Observacao = "Movimentacao manual"
        });

        if (avisosSemEstoque.Count == 0)
        {
            foreach (var movimento in BuildAutomaticConsumptionMovements(item, quantidadeUnidades))
            {
                _context.EstoqueMovimentos.Add(movimento);
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var nomeItem = item.Numero.HasValue ? $"{item.Nome} {item.Numero}" : item.Nome;
        var mensagem = avisosSemEstoque.Count == 0
            ? $"Item {nomeItem} adicionado ao estoque com consumo automatico dos componentes."
            : $"Item {nomeItem} adicionado ao estoque sem consumir disco/acessorios, conforme confirmacao.";

        return ResultadoMovimentoItem.ProcessadoComSucesso(mensagem);
    }

    private async Task<List<string>> BuildInsufficientStockWarningsAsync(Item item, int quantidadeUnidades)
    {
        var movements = await _context.EstoqueMovimentos
            .AsNoTracking()
            .ToListAsync();

        var avisos = new List<string>();

        var saldoDiscoPrincipal = CalculateBalanceKg(movements, EstoqueFormViewModel.TipoDisco, item.DiscoId);
        var discoPrincipalNecessario = item.Disco.PesoUnitarioKg * quantidadeUnidades;
        if (saldoDiscoPrincipal < discoPrincipalNecessario)
        {
            avisos.Add($"Disco principal {item.Disco.RaioMm} x {item.Disco.GrossuraMm:0.##} mm: precisa {discoPrincipalNecessario:0.####} kg e possui {saldoDiscoPrincipal:0.####} kg.");
        }

        if (item.PossuiTampa && item.DiscoTampa is not null && item.DiscoTampaId.HasValue)
        {
            var saldoDiscoTampa = CalculateBalanceKg(movements, EstoqueFormViewModel.TipoDisco, item.DiscoTampaId.Value);
            var discoTampaNecessario = item.DiscoTampa.PesoUnitarioKg * quantidadeUnidades;
            if (saldoDiscoTampa < discoTampaNecessario)
            {
                avisos.Add($"Disco da tampa {item.DiscoTampa.RaioMm} x {item.DiscoTampa.GrossuraMm:0.##} mm: precisa {discoTampaNecessario:0.####} kg e possui {saldoDiscoTampa:0.####} kg.");
            }
        }

        foreach (var itemAcessorio in item.ItemAcessorios)
        {
            var saldoAcessorio = CalculateBalanceKg(movements, EstoqueFormViewModel.TipoAcessorio, itemAcessorio.AcessorioId);
            var acessorioNecessario = itemAcessorio.Acessorio.PesoUnitarioKg * itemAcessorio.Quantidade * quantidadeUnidades;
            if (saldoAcessorio < acessorioNecessario)
            {
                avisos.Add($"Acessorio {itemAcessorio.Acessorio.Nome}: precisa {acessorioNecessario:0.####} kg e possui {saldoAcessorio:0.####} kg.");
            }
        }

        return avisos;
    }

    private static IEnumerable<EstoqueMovimento> BuildAutomaticConsumptionMovements(Item item, int quantidadeUnidades)
    {
        var movimentos = new List<EstoqueMovimento>
        {
            new()
            {
                Tipo = EstoqueFormViewModel.TipoDisco,
                Operacao = EstoqueFormViewModel.OperacaoSaida,
                DiscoId = item.DiscoId,
                Quantidade = item.Disco.PesoUnitarioKg * quantidadeUnidades,
                ConsumoAutomatico = true,
                Observacao = $"Consumo automatico ao adicionar item {ResolveItemName(item)}"
            }
        };

        if (item.PossuiTampa && item.DiscoTampaId.HasValue && item.DiscoTampa is not null)
        {
            movimentos.Add(new EstoqueMovimento
            {
                Tipo = EstoqueFormViewModel.TipoDisco,
                Operacao = EstoqueFormViewModel.OperacaoSaida,
                DiscoId = item.DiscoTampaId.Value,
                Quantidade = item.DiscoTampa.PesoUnitarioKg * quantidadeUnidades,
                ConsumoAutomatico = true,
                Observacao = $"Consumo automatico da tampa ao adicionar item {ResolveItemName(item)}"
            });
        }

        movimentos.AddRange(item.ItemAcessorios.Select(itemAcessorio => new EstoqueMovimento
        {
            Tipo = EstoqueFormViewModel.TipoAcessorio,
            Operacao = EstoqueFormViewModel.OperacaoSaida,
            AcessorioId = itemAcessorio.AcessorioId,
            Quantidade = itemAcessorio.Acessorio.PesoUnitarioKg * itemAcessorio.Quantidade * quantidadeUnidades,
            ConsumoAutomatico = true,
            Observacao = $"Consumo automatico ao adicionar item {ResolveItemName(item)}"
        }));

        return movimentos;
    }

    private async Task<decimal> GetCurrentBalanceKgAsync(string tipo, int referenciaId)
    {
        var movements = await _context.EstoqueMovimentos
            .AsNoTracking()
            .Where(m => m.Tipo == tipo && ResolveReferenceId(m) == referenciaId)
            .OrderBy(m => m.DataCriacaoUtc)
            .ThenBy(m => m.Id)
            .ToListAsync();

        return CalculateNonNegativeRunningBalance(movements);
    }

    private async Task<decimal> GetCurrentBalanceUnitsAsync(int itemId)
    {
        var movements = await _context.EstoqueMovimentos
            .AsNoTracking()
            .Where(m => m.Tipo == EstoqueFormViewModel.TipoItem && m.ItemId == itemId)
            .OrderBy(m => m.DataCriacaoUtc)
            .ThenBy(m => m.Id)
            .ToListAsync();

        return CalculateNonNegativeRunningBalance(movements);
    }

    private static decimal CalculateBalanceKg(IEnumerable<EstoqueMovimento> movements, string tipo, int referenciaId)
    {
        var filteredMovements = movements
            .Where(m => m.Tipo == tipo && ResolveReferenceId(m) == referenciaId)
            .OrderBy(m => m.DataCriacaoUtc)
            .ThenBy(m => m.Id)
            .ToList();

        return CalculateNonNegativeRunningBalance(filteredMovements);
    }

    private static decimal CalculateBalanceUnits(IEnumerable<EstoqueMovimento> movements, int itemId)
    {
        var filteredMovements = movements
            .Where(m => m.Tipo == EstoqueFormViewModel.TipoItem && m.ItemId == itemId)
            .OrderBy(m => m.DataCriacaoUtc)
            .ThenBy(m => m.Id)
            .ToList();

        return CalculateNonNegativeRunningBalance(filteredMovements);
    }

    private static decimal CalculateNonNegativeRunningBalance(IEnumerable<EstoqueMovimento> movements)
    {
        decimal saldo = 0;

        foreach (var movement in movements)
        {
            saldo += movement.Operacao == EstoqueFormViewModel.OperacaoEntrada
                ? movement.Quantidade
                : -movement.Quantidade;

            if (saldo < 0)
            {
                saldo = 0;
            }
        }

        return saldo;
    }

    private static int? ResolveReferenceId(EstoqueMovimento movement)
    {
        return movement.Tipo switch
        {
            EstoqueFormViewModel.TipoAcessorio => movement.AcessorioId,
            EstoqueFormViewModel.TipoDisco => movement.DiscoId,
            EstoqueFormViewModel.TipoItem => movement.ItemId,
            _ => null
        };
    }

    private static string ResolveMovementReference(EstoqueMovimento movement)
    {
        return movement.Tipo switch
        {
            EstoqueFormViewModel.TipoAcessorio => movement.Acessorio?.Nome ?? "-",
            EstoqueFormViewModel.TipoDisco => movement.Disco is null ? "-" : $"{movement.Disco.RaioMm} x {movement.Disco.GrossuraMm:0.##} mm",
            EstoqueFormViewModel.TipoItem => movement.Item is null ? "-" : ResolveItemName(movement.Item),
            _ => "-"
        };
    }

    private static string ResolveItemName(Item item)
    {
        return item.Numero.HasValue ? $"{item.Nome} {item.Numero}" : item.Nome;
    }

    private sealed record ResultadoMovimentoItem(bool Processado, string MensagemSucesso, EstoqueFormViewModel? FormularioRetorno)
    {
        public static ResultadoMovimentoItem ProcessadoComSucesso(string mensagem) => new(true, mensagem, null);
        public static ResultadoMovimentoItem PendenteConfirmacao(EstoqueFormViewModel form) => new(false, string.Empty, form);
    }
}
