using DocumentManagementSystem.Components;
using DocumentManagementSystem.Services;
using DocumentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Entity Framework Core - PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

// DatabaseConnection y Servicios
builder.Services.AddSingleton<DocumentManagementSystem.Model.DatabaseConnection>();
builder.Services.AddSingleton<DocumentService>(sp =>
    new DocumentService(
        sp.GetRequiredService<DocumentManagementSystem.Model.DatabaseConnection>(),
        sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<OfficeService>();
builder.Services.AddSingleton<DocumentService>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthenticationStateService>();
// ---------------------------------------------------------------------
// Razor Components / Blazor Server setup
// ---------------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();