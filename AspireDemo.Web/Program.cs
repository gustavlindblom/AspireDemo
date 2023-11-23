using AspireDemo.Web;
using AspireDemo.Web.Components;
using Constants = AspireDemo.Domain.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRedisOutputCache(Constants.ContainerNames.RedisContainer);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ProductApiClient>(client => client.BaseAddress = new($"http://{Constants.ServiceNames.ApiService}"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    
app.MapDefaultEndpoints();

app.Run();
