using System.Globalization;
using Fabrica.Data;
using Fabrica.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var verifySqlitePath = GetSqlitePathArgument(args, "--verify-sqlite");
if (verifySqlitePath is not null)
{
    await SqliteDataImporter.VerifyAsync(verifySqlitePath, Console.Out);
    return;
}

var importSqlitePath = GetSqlitePathArgument(args, "--import-sqlite");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A conexão 'ConnectionStrings:DefaultConnection' é obrigatória. " +
        "Configure-a nos User Secrets para desenvolvimento local ou nas configurações do App Service.");
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoginCacheService, LoginCacheService>();
var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new[] { cultureInfo };
    options.SupportedUICultures = new[] { cultureInfo };
});

var app = builder.Build();

if (importSqlitePath is not null)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseInitializer.Migrate(context);
    await SqliteDataImporter.ImportAsync(context, importSqlitePath, Console.Out);
    DatabaseInitializer.Initialize(app.Services);
    return;
}

DatabaseInitializer.Initialize(app.Services);

if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static string? GetSqlitePathArgument(string[] arguments, string option)
{
    var optionIndex = Array.FindIndex(arguments, argument => string.Equals(argument, option, StringComparison.OrdinalIgnoreCase));
    if (optionIndex < 0)
    {
        return null;
    }

    if (optionIndex == arguments.Length - 1 || string.IsNullOrWhiteSpace(arguments[optionIndex + 1]))
    {
        throw new ArgumentException($"Informe o caminho do arquivo SQLite após {option}.");
    }

    return arguments[optionIndex + 1];
}
