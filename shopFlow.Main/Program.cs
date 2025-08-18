using BlazorBootstrap;
using NLog;
using shopFlow.Components;
using shopFlow.Config;
using shopFlow.Importer.Providers;
using shopFlow.Persistency;
using shopFlow.Persistency.Impl;
using shopFlow.Services;
using shopFlow.Utils;
using System.IO.Abstractions;

var logLayout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=ToString}";
LogManager.Setup().LoadConfiguration(
    builder => { 
        builder.ForLogger().FilterMinLevel(NLog.LogLevel.Info).WriteToConsole(logLayout);
    });

var logger = LogManager.GetCurrentClassLogger();
logger.Info("Starting ShopFlow");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("/config/appsettings.Development.json", true, true);
ShopFlowConfig.SetConfigurationManger(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton(new LiteDB.LiteDatabase("/data/shopFlowMovs/lite.db"));
builder.Services.AddMovementPersistency();
builder.Services.AddExpensesPersistency();

builder.Services.AddScoped<IMovementService, DefaultMovementService>();
builder.Services.AddScoped<IExpenseService, DefaultExpenseService>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddHostedService<RecurringTaskService>();

var gdriveConfig = ShopFlowConfig.Instance.GDriveConfig;
var provider = new GDriveInvoicesLookup(gdriveConfig.CredentialPath, (gdriveConfig.PendingFolderId, gdriveConfig.ImportedFolderId), LoggerUtils.CreateLogger<GDriveInvoicesLookup>());
builder.Services.AddSingleton<IInvoicesLookup>(provider);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
