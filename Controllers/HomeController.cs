using System.Diagnostics;
using Fabrica.Models;
using Fabrica.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ILoginCacheService _loginCacheService;

    public HomeController(ILogger<HomeController> logger, ILoginCacheService loginCacheService)
    {
        _logger = logger;
        _loginCacheService = loginCacheService;
    }

    public IActionResult Index()
    {
        if (!TryGetLoggedUser(out var user))
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["LoggedUserName"] = user?.Name ?? user?.Email;
        return View();
    }

    public IActionResult Privacy()
    {
        if (!TryGetLoggedUser(out _))
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private bool TryGetLoggedUser(out LoggedUser? user)
    {
        var sessionId = HttpContext.Session.Id;
        if (_loginCacheService.TryGetLoggedUser(sessionId, out var loggedUser))
        {
            user = loggedUser;
            return true;
        }

        user = null;
        return false;
    }
}
