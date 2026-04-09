using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Models.ViewModels.Admin;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class AdminUsersController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public AdminUsersController(AppDbContext context, ILoginCacheService loginCacheService)
    {
        _context = context;
        _loginCacheService = loginCacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var redirect = EnsureAdminAccess(out _);
        if (redirect != null)
        {
            return redirect;
        }

        var model = await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.NivelAcesso)
            .OrderBy(u => u.Nome)
            .Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Nivel = u.NivelAcesso != null ? u.NivelAcesso.Nome : "N/A"
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var redirect = EnsureAdminAccess(out _);
        if (redirect != null)
        {
            return redirect;
        }

        var viewModel = new UserFormViewModel
        {
            NiveisAcesso = await GetNiveisAcessoSelectListAsync()
        };

        return View("Form", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        var redirect = EnsureAdminAccess(out _);
        if (redirect != null)
        {
            return redirect;
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Informe a senha.");
        }

        model.NiveisAcesso = await GetNiveisAcessoSelectListAsync(model.NivelAcessoId);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var exists = await _context.Usuarios.AnyAsync(u => u.Email == normalizedEmail);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View("Form", model);
        }

        var usuario = new Usuario
        {
            Nome = model.Nome.Trim(),
            Email = normalizedEmail,
            Senha = model.Password!.Trim(),
            NivelAcessoId = model.NivelAcessoId
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Usuário criado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var redirect = EnsureAdminAccess(out _);
        if (redirect != null)
        {
            return redirect;
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            TempData["StatusMessage"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new UserFormViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            NivelAcessoId = usuario.NivelAcessoId,
            NiveisAcesso = await GetNiveisAcessoSelectListAsync(usuario.NivelAcessoId)
        };

        return View("Form", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel model)
    {
        var redirect = EnsureAdminAccess(out _);
        if (redirect != null)
        {
            return redirect;
        }

        if (!model.Id.HasValue)
        {
            return BadRequest();
        }

        model.NiveisAcesso = await GetNiveisAcessoSelectListAsync(model.NivelAcessoId);

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var usuario = await _context.Usuarios.FindAsync(model.Id.Value);
        if (usuario is null)
        {
            TempData["StatusMessage"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var emailEmUso = await _context.Usuarios
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != usuario.Id);
        if (emailEmUso)
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View("Form", model);
        }

        usuario.Nome = model.Nome.Trim();
        usuario.Email = normalizedEmail;
        usuario.NivelAcessoId = model.NivelAcessoId;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            usuario.Senha = model.Password.Trim();
        }

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var redirect = EnsureAdminAccess(out var currentUser);
        if (redirect != null)
        {
            return redirect;
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            TempData["StatusMessage"] = "Usuário não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        if (currentUser is not null && currentUser.Id == usuario.Id)
        {
            TempData["StatusMessage"] = "Você não pode excluir o próprio usuário logado.";
            return RedirectToAction(nameof(Index));
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Usuário excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult? EnsureAdminAccess(out LoggedUser? user)
    {
        if (!_loginCacheService.TryGetLoggedUser(HttpContext.Session.Id, out user) || user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (user.NivelAcessoId != 1)
        {
            return RedirectToAction("Index", "Home");
        }

        return null;
    }

    private async Task<IEnumerable<SelectListItem>> GetNiveisAcessoSelectListAsync(int? selectedId = null)
    {
        var niveis = await _context.NiveisAcesso
            .AsNoTracking()
            .OrderBy(n => n.Nome)
            .ToListAsync();

        return niveis.Select(n => new SelectListItem
        {
            Value = n.Id.ToString(),
            Text = n.Nome,
            Selected = selectedId.HasValue && selectedId.Value == n.Id
        });
    }
}
