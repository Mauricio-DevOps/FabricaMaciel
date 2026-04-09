using Fabrica.Data;
using Fabrica.Models;
using Fabrica.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Controllers;

public class AccountController : Controller
{
    private const string SessionBootstrapKey = "__session_init";
    private const int DefaultNivelAcessoId = 2;

    private readonly AppDbContext _context;
    private readonly ILoginCacheService _loginCacheService;

    public AccountController(AppDbContext context, ILoginCacheService loginCacheService)
    {
        _context = context;
        _loginCacheService = loginCacheService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (IsLoggedIn())
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.Senha == model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        var sessionId = EnsureSession();
        var displayName = string.IsNullOrWhiteSpace(user.Nome) ? user.Email : user.Nome;
        _loginCacheService.SetLoggedUser(sessionId, new LoggedUser(user.Id, displayName, user.Email, user.NivelAcessoId));

        TempData["LoginMessage"] = "Login realizado com sucesso.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var alreadyExists = await _context.Usuarios.AnyAsync(u => u.Email == normalizedEmail);
        if (alreadyExists)
        {
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");
            return View(model);
        }

        var user = new Usuario
        {
            Nome = model.UserName.Trim(),
            Email = normalizedEmail,
            Senha = model.Password,
            NivelAcessoId = DefaultNivelAcessoId
        };

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        TempData["RegisterMessage"] = "Conta criada com sucesso! Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        var sessionId = EnsureSession();
        _loginCacheService.Remove(sessionId);
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private bool IsLoggedIn()
    {
        var sessionId = HttpContext.Session.Id;
        return _loginCacheService.TryGetLoggedUser(sessionId, out _);
    }

    private string EnsureSession()
    {
        if (!HttpContext.Session.TryGetValue(SessionBootstrapKey, out _))
        {
            HttpContext.Session.SetString(SessionBootstrapKey, DateTime.UtcNow.ToString("O"));
        }

        return HttpContext.Session.Id;
    }
}
