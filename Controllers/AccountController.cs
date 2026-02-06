using Fabrica.Models;
using Fabrica.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Controllers;

public class AccountController : Controller
{
    private const string SessionBootstrapKey = "__session_init";
    private const string ValidEmail = "teste@teste.com.br";
    private const string ValidPassword = "123";

    private readonly ILoginCacheService _loginCacheService;

    public AccountController(ILoginCacheService loginCacheService)
    {
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
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!IsValidCredentials(model.Email, model.Password))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        var sessionId = EnsureSession();
        var user = new LoggedUser("Usuário Teste", model.Email.Trim());
        _loginCacheService.SetLoggedUser(sessionId, user);

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
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ViewBag.Message = "Cadastro ainda não disponível. Em breve esta funcionalidade será liberada.";
        return View(model);
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

    private static bool IsValidCredentials(string email, string password) =>
        string.Equals(email?.Trim(), ValidEmail, StringComparison.OrdinalIgnoreCase) && password == ValidPassword;
}
