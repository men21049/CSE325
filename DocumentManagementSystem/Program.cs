using DocumentManagementSystem.Components;
using DocumentManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// SERVICES
builder.Services.AddSingleton<DocumentService>();
builder.Services.AddSingleton<OfficeService>();
builder.Services.AddSingleton<UserService>();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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