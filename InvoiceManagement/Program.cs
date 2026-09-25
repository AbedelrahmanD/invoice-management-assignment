using InvoiceManagement.Components;
using InvoiceManagement.Data;
using InvoiceManagement.Services.Implementations;
using InvoiceManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
 
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
   .AddInteractiveServerComponents();

builder.Services.AddValidation();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInvoiceService, InvoiceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = factory.CreateDbContext();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

