using BudgetTracker.Application;
using BudgetTracker.Domain;
using BudgetTracker.Infrastructure;
using BudgetTracker.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dataRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
builder.Services.AddSingleton<IMonthlyBudgetRepository>(_ =>
    new FileMonthlyBudgetRepository(Path.Combine(dataRoot, "worksheets")));
builder.Services.AddSingleton<IBucketBookRepository>(_ =>
    new FileBucketBookRepository(Path.Combine(dataRoot, "buckets")));
builder.Services.AddSingleton<BudgetWorkspace>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
