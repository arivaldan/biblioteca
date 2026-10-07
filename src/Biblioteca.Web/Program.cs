using Biblioteca.Web.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Dirección de la Api (RW-02). Está en appsettings.json para poder cambiarla sin tocar código.
// Termina en "/" para que las rutas relativas del cliente ("api/libros") se sumen bien.
string urlBaseApi = builder.Configuration["Api:UrlBase"]
    ?? throw new InvalidOperationException("Falta la dirección de la Api ('Api:UrlBase') en appsettings.");

// HttpClient tipado: ASP.NET Core crea y reutiliza las conexiones, y le pasa a
// LibrosApiClient un HttpClient que ya apunta a la Api.
builder.Services.AddHttpClient<LibrosApiClient>(cliente =>
{
    cliente.BaseAddress = new Uri(urlBaseApi);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
