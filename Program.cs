using AtlasViewer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios en DI
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<IRolService, RolService>();

// MVC y Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

// Mongo client
var mongoConfig = builder.Configuration.GetSection("Mongo");
var connectionString = mongoConfig.GetValue<string>("ConnectionString");
var databaseName = mongoConfig.GetValue<string>("Database");

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<AtlasViewer.Services.MongoService>();

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie(options =>
 {
 options.LoginPath = "/Login";
 options.AccessDeniedPath = "/Login";
 });

builder.Services.AddAuthorization(options =>
{
 options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

// Garantizar charset UTF-8 en respuestas HTML
app.Use(async (context, next) =>
{
 // Establecer/ajustar charset en el momento apropiado
 context.Response.OnStarting(() =>
 {
 var ct = context.Response.ContentType;
 if (!string.IsNullOrEmpty(ct) && ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
 {
 if (!ct.Contains("charset=", StringComparison.OrdinalIgnoreCase))
 {
 context.Response.ContentType = "text/html; charset=utf-8";
 }
 else if (!ct.Contains("utf-8", StringComparison.OrdinalIgnoreCase))
 {
 // Forzar utf-8 si algún proxy estableció otro charset
 context.Response.ContentType = "text/html; charset=utf-8";
 }
 }
 return Task.CompletedTask;
 });
 await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/Login"));
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
