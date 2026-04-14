using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Models.ViewModels.Items;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class ItemsController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public ItemsController(AppDbContext context, ILoginCacheService loginCacheService)
    {
        _context = context;
        _loginCacheService = loginCacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var items = await _context.Itens
            .AsNoTracking()
            .Include(i => i.Disco)
            .Include(i => i.DiscoTampa)
            .Include(i => i.ItemAcessorios)
                .ThenInclude(ia => ia.Acessorio)
            .OrderBy(i => i.Nome)
            .ThenBy(i => i.Numero)
            .ToListAsync();

        var model = new ItemsIndexViewModel
        {
            Itens = items
                .Select(i => new ItemListItemViewModel
                {
                    Id = i.Id,
                    Nome = i.Nome,
                    Numero = i.Numero,
                    DiscoPrincipal = FormatDisk(i.Disco),
                    PossuiTampa = i.PossuiTampa,
                    DiscoTampa = i.DiscoTampa is null ? null : FormatDisk(i.DiscoTampa),
                    AcessoriosResumo = BuildAccessoriesSummary(i.ItemAcessorios),
                    PrecoPromocional = i.PrecoPromocional,
                    PrecoAtacado = i.PrecoAtacado,
                    PrecoVarejo = i.PrecoVarejo
                })
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var model = await BuildFormModelAsync(new ItemFormViewModel());
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemFormViewModel model)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        await PopulateFormDependenciesAsync(model);
        ValidateForm(model);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var entity = new Item
        {
            Nome = model.Nome.Trim(),
            Numero = model.Numero,
            DiscoId = model.DiscoId,
            PossuiTampa = model.PossuiTampa,
            DiscoTampaId = model.PossuiTampa ? model.DiscoTampaId : null,
            PrecoPromocional = model.PrecoPromocional,
            PrecoAtacado = model.PrecoAtacado,
            PrecoVarejo = model.PrecoVarejo,
            ItemAcessorios = GetSelectedAccessories(model)
                .Select(a => new ItemAcessorio
                {
                    AcessorioId = a.AcessorioId,
                    Quantidade = a.Quantidade
                })
                .ToList()
        };

        _context.Itens.Add(entity);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Item criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var item = await _context.Itens
            .AsNoTracking()
            .Include(i => i.ItemAcessorios)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item is null)
        {
            TempData["StatusMessage"] = "Item nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ItemFormViewModel
        {
            Id = item.Id,
            Nome = item.Nome,
            Numero = item.Numero,
            DiscoId = item.DiscoId,
            PossuiTampa = item.PossuiTampa,
            DiscoTampaId = item.DiscoTampaId,
            PrecoPromocional = item.PrecoPromocional,
            PrecoAtacado = item.PrecoAtacado,
            PrecoVarejo = item.PrecoVarejo
        };

        await PopulateFormDependenciesAsync(model, item.ItemAcessorios);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ItemFormViewModel model)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        await PopulateFormDependenciesAsync(model);
        ValidateForm(model);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var item = await _context.Itens
            .Include(i => i.ItemAcessorios)
            .FirstOrDefaultAsync(i => i.Id == model.Id.Value);

        if (item is null)
        {
            TempData["StatusMessage"] = "Item nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        item.Nome = model.Nome.Trim();
        item.Numero = model.Numero;
        item.DiscoId = model.DiscoId;
        item.PossuiTampa = model.PossuiTampa;
        item.DiscoTampaId = model.PossuiTampa ? model.DiscoTampaId : null;
        item.PrecoPromocional = model.PrecoPromocional;
        item.PrecoAtacado = model.PrecoAtacado;
        item.PrecoVarejo = model.PrecoVarejo;

        _context.ItemAcessorios.RemoveRange(item.ItemAcessorios);
        item.ItemAcessorios = GetSelectedAccessories(model)
            .Select(a => new ItemAcessorio
            {
                ItemId = item.Id,
                AcessorioId = a.AcessorioId,
                Quantidade = a.Quantidade
            })
            .ToList();

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Item atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var item = await _context.Itens.FindAsync(id);
        if (item is null)
        {
            TempData["StatusMessage"] = "Item nao encontrado.";
            return RedirectToAction(nameof(Index));
        }

        _context.Itens.Remove(item);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Item excluido com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureAdminAccess()
    {
        if (!_loginCacheService.TryGetLoggedUser(HttpContext.Session.Id, out var user) || user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (user.NivelAcessoId != 1)
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private async Task<ItemFormViewModel> BuildFormModelAsync(ItemFormViewModel model, IEnumerable<ItemAcessorio>? selectedAccessories = null)
    {
        await PopulateFormDependenciesAsync(model, selectedAccessories);
        return model;
    }

    private async Task PopulateFormDependenciesAsync(ItemFormViewModel model, IEnumerable<ItemAcessorio>? selectedAccessories = null)
    {
        var disks = (await _context.Discos
            .AsNoTracking()
            .ToListAsync())
            .OrderBy(d => d.RaioMm)
            .ThenBy(d => d.GrossuraMm)
            .ToList();

        model.DiscoOptions = disks
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = $"{d.RaioMm} x {d.GrossuraMm:0.##} mm - {d.PesoUnitarioKg:0.0000} kg"
            })
            .ToList();

        var selectedMap = selectedAccessories?
            .ToDictionary(a => a.AcessorioId, a => a.Quantidade)
            ?? model.Acessorios
                .Where(a => a.Selecionado)
                .ToDictionary(a => a.AcessorioId, a => a.Quantidade);

        var accessories = await _context.Acessorios
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync();

        model.Acessorios = accessories
            .Select(a => new ItemAccessorySelectionViewModel
            {
                AcessorioId = a.Id,
                Nome = a.Nome,
                Descricao = a.Descricao,
                PesoUnitarioGramas = a.PesoUnitarioKg * 1000m,
                Selecionado = selectedMap.ContainsKey(a.Id),
                Quantidade = selectedMap.TryGetValue(a.Id, out var quantidade) ? quantidade : 1
            })
            .ToList();
    }

    private void ValidateForm(ItemFormViewModel model)
    {
        if (!_context.Discos.Any(d => d.Id == model.DiscoId))
        {
            ModelState.AddModelError(nameof(model.DiscoId), "Selecione o disco principal valido.");
        }

        if (!model.PossuiTampa)
        {
            model.DiscoTampaId = null;
        }
        else
        {
            if (!model.DiscoTampaId.HasValue)
            {
                ModelState.AddModelError(nameof(model.DiscoTampaId), "Selecione o disco da tampa.");
            }
            else if (!_context.Discos.Any(d => d.Id == model.DiscoTampaId.Value))
            {
                ModelState.AddModelError(nameof(model.DiscoTampaId), "Selecione um disco de tampa valido.");
            }
        }

        foreach (var accessory in model.Acessorios.Where(a => a.Selecionado))
        {
            if (accessory.Quantidade < 1)
            {
                ModelState.AddModelError(string.Empty, $"A quantidade do acessorio {accessory.Nome} deve ser maior que zero.");
            }
        }
    }

    private static IReadOnlyList<ItemAccessorySelectionViewModel> GetSelectedAccessories(ItemFormViewModel model)
    {
        return model.Acessorios
            .Where(a => a.Selecionado)
            .ToList();
    }

    private static string FormatDisk(Disco disk)
    {
        return $"{disk.RaioMm} x {disk.GrossuraMm:0.##} mm";
    }

    private static string BuildAccessoriesSummary(IEnumerable<ItemAcessorio> accessories)
    {
        var selected = accessories
            .OrderBy(a => a.Acessorio.Nome)
            .Select(a => $"{a.Quantidade}x {a.Acessorio.Nome}")
            .ToList();

        return selected.Count == 0 ? "-" : string.Join(", ", selected);
    }
}
