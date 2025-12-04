using DocumentManagementSystem.Components;
using DocumentManagementSystem.Services;  // <-- required for services

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// REGISTER APPLICATION SERVICES (Document, Office, User)
// ---------------------------------------------------------------------
builder.Services.AddSingleton<DocumentManagementSystem.Model.DatabaseConnection>();
builder.Services.AddSingleton<DocumentService>();
builder.Services.AddSingleton<OfficeService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<BlobStorageService>();

// ---------------------------------------------------------------------
// Razor Components / Blazor Server setup
// ---------------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ---------------------------------------------------------------------
// HTTP REQUEST PIPELINE
// ---------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();  // Enforces HTTPS for production
}

app.UseHttpsRedirection();
app.UseAntiforgery();   // CSRF protection

// ---------------------------------------------------------------------
// Static Files + Routing
// ---------------------------------------------------------------------
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ---------------------------------------------------------------------

app.Run();