using AtlasViewer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using Microsoft.AspNetCore.StaticFiles;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configurar encoding UTF-8 y cultura española
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-ES");

// Registrar servicios en DI
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<IRolService, RolService>();

// MVC y Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
 // Requiere autenticación para todo
 options.Conventions.AuthorizeFolder("/");
 
 // Solo administradores pueden acceder a estas secciones
 options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Usuarios", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Roles", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Tools", "AdminOnly");
 
 // Permitir acceso sin autenticación a Login y Logout
 options.Conventions.AllowAnonymousToPage("/Login");
 options.Conventions.AllowAnonymousToPage("/Logout");
 options.Conventions.AllowAnonymousToPage("/Index");
 options.Conventions.AllowAnonymousToPage("/AccessDenied");
})
.AddViewOptions(options =>
{
 options.HtmlHelperOptions.ClientValidationEnabled = true;
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
 options.AccessDeniedPath = "/AccessDenied";
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

// Force UTF-8 for static files (e.g., any HTML under wwwroot if present)
var provider = new FileExtensionContentTypeProvider();
app.UseStaticFiles(new StaticFileOptions
{
 ContentTypeProvider = provider,
 ServeUnknownFileTypes = true,
 DefaultContentType = "text/html; charset=utf-8"
});

// Middleware robusto: fuerza header Content-Type con charset utf-8 para HTML
app.Use(async (context, next) =>
{
 await next();

 var path = context.Request.Path.Value ?? string.Empty;
 var isHtmlRoute = string.IsNullOrEmpty(System.IO.Path.GetExtension(path)) || path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);

 if (isHtmlRoute)
 {
 var ct = context.Response.ContentType;
 if (string.IsNullOrEmpty(ct))
 {
 context.Response.ContentType = "text/html; charset=utf-8";
 }
 else if (ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) && !ct.Contains("charset=", StringComparison.OrdinalIgnoreCase))
 {
 context.Response.ContentType = "text/html; charset=utf-8";
 }
 }
});

// Endpoint diagnóstico de cultura
app.MapGet("/diag/culture", () =>
{
 var current = CultureInfo.CurrentCulture.Name;
 var ui = CultureInfo.CurrentUICulture.Name;
 return Results.Content($"Culture: {current}, UICulture: {ui}", "text/plain; charset=utf-8");
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/Login"));
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
