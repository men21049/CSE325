using DocumentManagementSystem.Components;
using DocumentManagementSystem.Services;


var builder = WebApplication.CreateBuilder(args);

// DatabaseConnection and Services
builder.Services.AddSingleton<DocumentManagementSystem.Model.DatabaseConnection>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<DocumentService>(sp =>
    new DocumentService(
        sp.GetRequiredService<DocumentManagementSystem.Model.DatabaseConnection>(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<BlobStorageService>()));
builder.Services.AddSingleton<OfficeService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<AuthenticationStateService>();
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

// Document download endpoint
app.MapGet("/api/download/{documentId}", async (int documentId, DocumentService documentService) =>
{
    try
    {
        var (stream, fileName, contentType) = await documentService.GetDocumentStreamAsync(documentId);
        
        return Results.File(
            stream,
            contentType: contentType,
            fileDownloadName: fileName
        );
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error downloading document: {ex.Message}");
    }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();