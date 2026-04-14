using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Models.ViewModels.Pedidos;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class PedidosController : Controller
{
    private const string NovoClienteOptionValue = "__new__";

    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public PedidosController(AppDbContext context, ILoginCacheService loginCacheService)
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

        var pedidos = await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        var model = new PedidosIndexViewModel
        {
            Pedidos = pedidos
                .Select(p => new PedidoListItemViewModel
                {
                    Id = p.Id,
                    ClienteNome = p.Cliente.Nome,
                    DataPedido = p.DataPedido,
                    Status = p.Status,
                    QuantidadeItens = p.Itens.Sum(i => i.Quantidade),
                    TotalPedido = p.Itens.Sum(i => i.Quantidade * i.ValorUnitario)
                })
                .ToList(),
            StatusOptions = BuildStatusOptions(),
            QuantidadeEmNegociacao = pedidos.Count(p => p.Status == PedidoStatus.EmNegociacao),
            QuantidadeEmProducao = pedidos.Count(p => p.Status == PedidoStatus.EmProducao),
            QuantidadeFinalizados = pedidos.Count(p => p.Status == PedidoStatus.Finalizado),
            QuantidadeEntregues = pedidos.Count(p => p.Status == PedidoStatus.Entregue)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var model = await BuildFormModelAsync(new PedidoFormViewModel
        {
            Itens = [new PedidoFormItemViewModel()]
        });

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PedidoFormViewModel model)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        await PopulateFormDependenciesAsync(model);
        await ValidateFormAsync(model);

        if (!ModelState.IsValid)
        {
            EnsureAtLeastOneItemLine(model);
            return View("Form", model);
        }

        var itens = GetItensSelecionados(model);

        var pedido = new Pedido
        {
            ClienteId = model.ClienteId!.Value,
            DataPedido = model.DataPedido.Date,
            Status = model.Status,
            Itens = itens
                .Select(i => new PedidoItem
                {
                    ItemId = i.ItemId!.Value,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario!.Value
                })
                .ToList()
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Pedido criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var pedido = await _context.Pedidos
            .AsNoTracking()
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
        {
            TempData["StatusMessage"] = "Pedido nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PedidoFormViewModel
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            DataPedido = pedido.DataPedido,
            Status = pedido.Status,
            Itens = pedido.Itens
                .OrderBy(i => i.Id)
                .Select(i => new PedidoFormItemViewModel
                {
                    ItemId = i.ItemId,
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario
                })
                .ToList()
        };

        await PopulateFormDependenciesAsync(model);
        EnsureAtLeastOneItemLine(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PedidoFormViewModel model)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        await PopulateFormDependenciesAsync(model);
        await ValidateFormAsync(model);

        if (!ModelState.IsValid)
        {
            EnsureAtLeastOneItemLine(model);
            return View("Form", model);
        }

        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == model.Id.Value);

        if (pedido is null)
        {
            TempData["StatusMessage"] = "Pedido nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var itens = GetItensSelecionados(model);

        pedido.ClienteId = model.ClienteId!.Value;
        pedido.DataPedido = model.DataPedido.Date;
        pedido.Status = model.Status;

        _context.PedidoItens.RemoveRange(pedido.Itens);
        pedido.Itens = itens
            .Select(i => new PedidoItem
            {
                PedidoId = pedido.Id,
                ItemId = i.ItemId!.Value,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario!.Value
            })
            .ToList();

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Pedido atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null)
        {
            TempData["StatusMessage"] = "Pedido nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Pedido excluido com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return redirect;
        }

        if (!PedidoStatus.IsValid(status))
        {
            TempData["StatusMessage"] = "Status de pedido invalido.";
            return RedirectToAction(nameof(Index));
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null)
        {
            TempData["StatusMessage"] = "Pedido nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        pedido.Status = status;
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Status do pedido atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient([Bind(Prefix = "NovoCliente")] NovoClienteViewModel model)
    {
        var redirect = EnsureLoggedAccess();
        if (redirect != null)
        {
            return Json(new
            {
                success = false,
                errors = new[] { "Sua sessao expirou. Entre novamente para cadastrar um cliente." }
            });
        }

        if (!TabelaPreco.IsValid(model.TabelaPreco))
        {
            ModelState.AddModelError(nameof(model.TabelaPreco), "Selecione uma tabela de preco valida.");
        }

        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct()
                    .ToArray()
            });
        }

        var cliente = new Cliente
        {
            Nome = model.Nome.Trim(),
            Endereco = model.Endereco.Trim(),
            Telefone = model.Telefone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            TabelaPreco = model.TabelaPreco
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            client = new
            {
                id = cliente.Id,
                nome = cliente.Nome,
                endereco = cliente.Endereco,
                telefone = cliente.Telefone,
                email = cliente.Email,
                tabelaPreco = cliente.TabelaPreco
            }
        });
    }

    private IActionResult? EnsureLoggedAccess()
    {
        if (!_loginCacheService.TryGetLoggedUser(HttpContext.Session.Id, out var user) || user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return null;
    }

    private async Task<PedidoFormViewModel> BuildFormModelAsync(PedidoFormViewModel model)
    {
        await PopulateFormDependenciesAsync(model);
        EnsureAtLeastOneItemLine(model);
        return model;
    }

    private async Task PopulateFormDependenciesAsync(PedidoFormViewModel model)
    {
        model.NovoCliente.TabelaPreco = TabelaPreco.IsValid(model.NovoCliente.TabelaPreco)
            ? model.NovoCliente.TabelaPreco
            : TabelaPreco.Varejo;

        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        model.ClienteOptions =
        [
            new SelectListItem("Criar novo cliente", NovoClienteOptionValue),
            .. clientes.Select(cliente => new SelectListItem(cliente.Nome, cliente.Id.ToString()))
        ];

        model.ClientesCatalogo = clientes
            .Select(cliente => new PedidoClienteOptionViewModel
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Endereco = cliente.Endereco,
                Telefone = cliente.Telefone,
                Email = cliente.Email,
                TabelaPreco = cliente.TabelaPreco
            })
            .ToList();

        var itens = await _context.Itens
            .AsNoTracking()
            .OrderBy(i => i.Nome)
            .ThenBy(i => i.Numero)
            .ToListAsync();

        model.ItemOptions = itens
            .Select(item => new SelectListItem(ResolveItemName(item), item.Id.ToString()))
            .ToList();

        model.ItensCatalogo = itens
            .Select(item => new PedidoItemCatalogOptionViewModel
            {
                Id = item.Id,
                Nome = ResolveItemName(item),
                PrecoPromocional = item.PrecoPromocional,
                PrecoAtacado = item.PrecoAtacado,
                PrecoVarejo = item.PrecoVarejo
            })
            .ToList();

        model.StatusOptions = BuildStatusOptions(model.Status);
        model.TabelaPrecoOptions = BuildTabelaPrecoOptions(model.NovoCliente.TabelaPreco);
    }

    private async Task ValidateFormAsync(PedidoFormViewModel model)
    {
        Cliente? cliente = null;

        if (!model.ClienteId.HasValue)
        {
            ModelState.AddModelError(nameof(model.ClienteId), "Selecione um cliente.");
        }
        else
        {
            cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == model.ClienteId.Value);

            if (cliente is null)
            {
                ModelState.AddModelError(nameof(model.ClienteId), "Selecione um cliente valido.");
            }
            else if (!TabelaPreco.IsValid(cliente.TabelaPreco))
            {
                ModelState.AddModelError(nameof(model.ClienteId), "O cliente selecionado nao possui uma tabela de preco valida.");
            }
        }

        if (!PedidoStatus.IsValid(model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "Selecione um status valido.");
        }

        var itensSelecionados = GetItensSelecionadosComIndice(model);
        if (itensSelecionados.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Adicione pelo menos um item ao pedido.");
            return;
        }

        var itemIdsSelecionados = itensSelecionados
            .Where(entry => entry.Item.ItemId.HasValue)
            .Select(entry => entry.Item.ItemId!.Value)
            .Distinct()
            .ToList();

        var itensExistentes = await _context.Itens
            .AsNoTracking()
            .Where(i => itemIdsSelecionados.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

        var itemIds = new HashSet<int>();

        foreach (var (item, index) in itensSelecionados)
        {
            var prefix = $"Itens[{index}]";

            if (!item.ItemId.HasValue)
            {
                ModelState.AddModelError($"{prefix}.ItemId", "Selecione um item.");
                continue;
            }

            if (!itensExistentes.TryGetValue(item.ItemId.Value, out var itemCadastrado))
            {
                ModelState.AddModelError($"{prefix}.ItemId", "Selecione um item valido.");
                continue;
            }

            if (item.Quantidade < 1)
            {
                ModelState.AddModelError($"{prefix}.Quantidade", "Informe uma quantidade valida.");
            }

            var precoTabela = cliente is null
                ? null
                : TabelaPreco.ObterPreco(itemCadastrado, cliente.TabelaPreco);

            if ((!item.ValorUnitario.HasValue || item.ValorUnitario.Value <= 0) &&
                precoTabela.HasValue &&
                precoTabela.Value > 0)
            {
                item.ValorUnitario = precoTabela.Value;
            }

            if (!item.ValorUnitario.HasValue || item.ValorUnitario.Value <= 0)
            {
                if (cliente is not null && !precoTabela.HasValue)
                {
                    ModelState.AddModelError(
                        $"{prefix}.ValorUnitario",
                        "O item selecionado nao possui preco cadastrado para esse cliente. Informe o valor unitario ou cadastre o preco correspondente no item.");
                }
                else
                {
                    ModelState.AddModelError($"{prefix}.ValorUnitario", "Informe um valor unitario valido.");
                }
            }

            if (!itemIds.Add(item.ItemId.Value))
            {
                ModelState.AddModelError(string.Empty, "Nao repita o mesmo item no pedido. Ajuste a quantidade na mesma linha.");
            }
        }
    }

    private static List<SelectListItem> BuildStatusOptions(string? selectedStatus = null)
    {
        return PedidoStatus.Todos
            .Select(status => new SelectListItem(status, status, status == selectedStatus))
            .ToList();
    }

    private static List<SelectListItem> BuildTabelaPrecoOptions(string? selectedValue = null)
    {
        return TabelaPreco.Todas
            .Select(tabela => new SelectListItem(tabela, tabela, tabela == selectedValue))
            .ToList();
    }

    private static List<PedidoFormItemViewModel> GetItensSelecionados(PedidoFormViewModel model)
    {
        return GetItensSelecionadosComIndice(model)
            .Select(entry => entry.Item)
            .ToList();
    }

    private static List<(PedidoFormItemViewModel Item, int Index)> GetItensSelecionadosComIndice(PedidoFormViewModel model)
    {
        return model.Itens
            .Select((item, index) => (Item: item, Index: index))
            .Where(entry =>
                entry.Item.ItemId.HasValue ||
                entry.Item.ValorUnitario.HasValue ||
                entry.Item.Quantidade != 1)
            .ToList();
    }

    private static void EnsureAtLeastOneItemLine(PedidoFormViewModel model)
    {
        if (model.Itens.Count == 0)
        {
            model.Itens.Add(new PedidoFormItemViewModel());
        }
    }

    private static string ResolveItemName(Item item)
    {
        return item.Numero.HasValue ? $"{item.Nome} {item.Numero}" : item.Nome;
    }
}
