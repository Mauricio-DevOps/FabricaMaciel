using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Models.ViewModels.Materials;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class MaterialsController : Controller
{
    private const string AccessoriesTab = "acessorios";
    private const string DiscsTab = "discos";
    private const decimal GramsPerKilogram = 1000m;

    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public MaterialsController(AppDbContext context, ILoginCacheService loginCacheService)
    {
        _context = context;
        _loginCacheService = loginCacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string tab = AccessoriesTab)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var normalizedTab = NormalizeTab(tab);
        var accessories = await _context.Acessorios
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .Select(a => new AccessoryListItemViewModel
            {
                Id = a.Id,
                Nome = a.Nome,
                Descricao = a.Descricao,
                PesoUnitarioGramas = a.PesoUnitarioKg * GramsPerKilogram
            })
            .ToListAsync();

        var discs = await _context.Discos
            .AsNoTracking()
            .Select(d => new DiskListItemViewModel
            {
                Id = d.Id,
                RaioMm = d.RaioMm,
                GrossuraMm = d.GrossuraMm,
                PesoUnitarioKg = d.PesoUnitarioKg
            })
            .ToListAsync();

        var model = new MaterialsIndexViewModel
        {
            SelectedTab = normalizedTab,
            Acessorios = accessories,
            Discos = discs
                .OrderBy(d => d.RaioMm)
                .ThenBy(d => d.GrossuraMm)
                .ThenBy(d => d.PesoUnitarioKg)
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateAccessory()
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        return View("AccessoryForm", new AccessoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccessory(AccessoryFormViewModel model)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return View("AccessoryForm", model);
        }

        var entity = new Acessorio
        {
            Nome = model.Nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(model.Descricao) ? null : model.Descricao.Trim(),
            PesoUnitarioKg = model.PesoUnitarioGramas / GramsPerKilogram
        };

        _context.Acessorios.Add(entity);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Acessório criado com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
    }

    [HttpGet]
    public async Task<IActionResult> EditAccessory(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var accessory = await _context.Acessorios.FindAsync(id);
        if (accessory is null)
        {
            TempData["StatusMessage"] = "Acessório não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
        }

        var model = new AccessoryFormViewModel
        {
            Id = accessory.Id,
            Nome = accessory.Nome,
            Descricao = accessory.Descricao,
            PesoUnitarioGramas = accessory.PesoUnitarioKg * GramsPerKilogram
        };

        return View("AccessoryForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccessory(AccessoryFormViewModel model)
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

        if (!ModelState.IsValid)
        {
            return View("AccessoryForm", model);
        }

        var accessory = await _context.Acessorios.FindAsync(model.Id.Value);
        if (accessory is null)
        {
            TempData["StatusMessage"] = "Acessório não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
        }

        accessory.Nome = model.Nome.Trim();
        accessory.Descricao = string.IsNullOrWhiteSpace(model.Descricao) ? null : model.Descricao.Trim();
        accessory.PesoUnitarioKg = model.PesoUnitarioGramas / GramsPerKilogram;

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Acessório atualizado com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccessory(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var accessory = await _context.Acessorios.FindAsync(id);
        if (accessory is null)
        {
            TempData["StatusMessage"] = "Acessório não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
        }

        _context.Acessorios.Remove(accessory);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Acessório excluído com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = AccessoriesTab });
    }

    [HttpGet]
    public IActionResult CreateDisk()
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        return View("DiskForm", new DiskFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDisk(DiskFormViewModel model)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        if (!ModelState.IsValid)
        {
            return View("DiskForm", model);
        }

        ApplyAutomaticDiskWeight(model);

        var entity = new Disco
        {
            RaioMm = model.RaioMm,
            GrossuraMm = model.GrossuraMm,
            PesoUnitarioKg = model.PesoUnitarioKg
        };

        _context.Discos.Add(entity);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Disco criado com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = DiscsTab });
    }

    [HttpGet]
    public async Task<IActionResult> EditDisk(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var disk = await _context.Discos.FindAsync(id);
        if (disk is null)
        {
            TempData["StatusMessage"] = "Disco não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = DiscsTab });
        }

        var model = new DiskFormViewModel
        {
            Id = disk.Id,
            RaioMm = disk.RaioMm,
            GrossuraMm = disk.GrossuraMm,
            PesoUnitarioKg = disk.PesoUnitarioKg,
            CalcularPesoAutomaticamente = true
        };

        return View("DiskForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDisk(DiskFormViewModel model)
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

        if (!ModelState.IsValid)
        {
            return View("DiskForm", model);
        }

        ApplyAutomaticDiskWeight(model);

        var disk = await _context.Discos.FindAsync(model.Id.Value);
        if (disk is null)
        {
            TempData["StatusMessage"] = "Disco não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = DiscsTab });
        }

        disk.RaioMm = model.RaioMm;
        disk.GrossuraMm = model.GrossuraMm;
        disk.PesoUnitarioKg = model.PesoUnitarioKg;

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Disco atualizado com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = DiscsTab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDisk(int id)
    {
        var redirect = EnsureAdminAccess();
        if (redirect != null)
        {
            return redirect;
        }

        var disk = await _context.Discos.FindAsync(id);
        if (disk is null)
        {
            TempData["StatusMessage"] = "Disco não encontrado.";
            return RedirectToAction(nameof(Index), new { tab = DiscsTab });
        }

        _context.Discos.Remove(disk);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Disco excluído com sucesso.";
        return RedirectToAction(nameof(Index), new { tab = DiscsTab });
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

    private static string NormalizeTab(string? tab)
    {
        return tab?.ToLowerInvariant() switch
        {
            DiscsTab => DiscsTab,
            _ => AccessoriesTab
        };
    }

    private static void ApplyAutomaticDiskWeight(DiskFormViewModel model)
    {
        if (!model.CalcularPesoAutomaticamente)
        {
            return;
        }

        model.PesoUnitarioKg = model.PesoUnitarioKgEstimado;
    }
}
