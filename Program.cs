using AtlasViewer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.HttpOverrides;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// Configurar ForwardedHeaders para proxies inversos (Render, Nginx, etc.)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Deshabilitar file watchers en producción (previene error inotify en Linux)
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Configurar encoding UTF-8 y cultura española
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-ES");

// Configurar cookies globales
builder.Services.Configure<CookiePolicyOptions>(options =>
{
 options.MinimumSameSitePolicy = SameSiteMode.Lax; // Cambiado de Strict a Lax para compatibilidad
 options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
 options.Secure = builder.Environment.IsDevelopment() 
     ? CookieSecurePolicy.None 
     : CookieSecurePolicy.Always;
});

// Registrar servicios en DI
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));

// Configurar MongoDB
var mongoConfig = builder.Configuration.GetSection("Mongo");
var connectionString = mongoConfig.GetValue<string>("ConnectionString");
var databaseName = mongoConfig.GetValue<string>("Database");

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});
builder.Services.AddSingleton<MongoService>();

// Servicios de negocio
builder.Services.AddSingleton<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<IRolService, RolService>();

// Servicios de catálogos
builder.Services.AddSingleton<IPescadorService, PescadorService>();
builder.Services.AddSingleton<IEmbarcacionService, EmbarcacionService>();
builder.Services.AddSingleton<IEspecieService, EspecieService>();
builder.Services.AddSingleton<IInsumoService, InsumoService>();
builder.Services.AddSingleton<IProveedorService, ProveedorService>();
builder.Services.AddSingleton<IArtePescaService, ArtePescaService>();
builder.Services.AddSingleton<ISitioPescaService, SitioPescaService>();

// Servicios operativos
builder.Services.AddSingleton<ICompraInsumoService, CompraInsumoService>();
builder.Services.AddSingleton<IRegistroCapturaService, RegistroCapturaService>();
builder.Services.AddSingleton<ICapturaDetalleService, CapturaDetalleService>();
builder.Services.AddSingleton<IMonitoreoBiologicoService, MonitoreoBiologicoService>();
builder.Services.AddSingleton<IPescaIncidentalService, PescaIncidentalService>();
builder.Services.AddSingleton<IPescaFantasmaService, PescaFantasmaService>();
builder.Services.AddSingleton<IVentaEspecieService, VentaEspecieService>();

// MVC y Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
 // Requiere autenticación para todo
 options.Conventions.AuthorizeFolder("/");
 
 // Permitir acceso anónimo a páginas públicas (crítico para evitar bucles)
 options.Conventions.AllowAnonymousToPage("/Login");
 options.Conventions.AllowAnonymousToPage("/Logout");
 options.Conventions.AllowAnonymousToPage("/Index");
 
 // Solo administradores pueden acceder a estas secciones
 options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Usuarios", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Roles", "AdminOnly");
 options.Conventions.AuthorizeFolder("/Tools", "AdminOnly");
 
 // Editor y Administrador pueden acceder a gestión de datos
 options.Conventions.AuthorizeFolder("/Pescadores", "EditorOrAdmin");
 options.Conventions.AuthorizeFolder("/Embarcaciones", "EditorOrAdmin");
});

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie(options =>
 {
 options.LoginPath = "/Login";
 options.AccessDeniedPath = "/AccessDenied";
 options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
     ? CookieSecurePolicy.None 
     : CookieSecurePolicy.Always;
 options.Cookie.HttpOnly = true;
 options.Cookie.SameSite = SameSiteMode.Lax; // Cambiado de Strict a Lax
 });

builder.Services.AddAuthorization(options =>
{
 options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
 options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Administrador", "Editor"));
});

// Configurar HSTS
builder.Services.AddHsts(options =>
{
 options.Preload = true;
 options.IncludeSubDomains = true;
 options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// IMPORTANTE: UseForwardedHeaders debe ser el primer middleware
app.UseForwardedHeaders();

// Middleware de cabeceras de seguridad
app.Use(async (context, next) =>
{
 // Generar nonce único para CSP
 var nonce = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
 context.Items["csp-nonce"] = nonce;
 
 // Control de caché para páginas sensibles
 var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
 var isSensitivePage = path.Contains("/login") || 
                       path.Contains("/account") || 
                       path.Contains("/admin") || 
                       path.Contains("/usuarios") || 
                       path.Contains("/roles");
 
 if (isSensitivePage)
 {
 context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
 context.Response.Headers.Append("Pragma", "no-cache");
 context.Response.Headers.Append("Expires", "0");
 }
 
 // HSTS - HTTP Strict Transport Security
 context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
 
 // CSP - Content Security Policy con nonce
 context.Response.Headers.Append("Content-Security-Policy", 
 $"default-src 'none'; " +
 $"script-src 'self' 'nonce-{nonce}' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
 $"style-src 'self' 'unsafe-hashes' 'nonce-{nonce}' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
 $"img-src 'self' data:; " +
 $"font-src 'self' data: https://cdn.jsdelivr.net; " +
 $"connect-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
 $"frame-ancestors 'self'; " +
 $"base-uri 'self'; " +
 $"form-action 'self'; " +
 $"object-src 'none'; " +
 $"manifest-src 'self'; " +
 $"media-src 'none'; " +
 $"worker-src 'none'; " +
 $"frame-src 'none'");
 
 // X-Frame-Options - Previene clickjacking
 context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
 
 // X-Content-Type-Options - Previene MIME sniffing
 context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
 
 // Referrer-Policy - Control de información de referencia
 context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
 
 // Permissions-Policy - Control de APIs del navegador
 context.Response.Headers.Append("Permissions-Policy", 
 "geolocation=(), microphone=(), camera=(), payment=()");
 
 // Cross-Origin-Embedder-Policy
 context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
 
 // Cross-Origin-Opener-Policy
 context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
 
 // Cross-Origin-Resource-Policy
 context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
 
 // X-XSS-Protection (legacy pero útil)
 context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
 
 // Remover header Server para no exponer información
 context.Response.Headers.Remove("Server");
 context.Response.Headers.Remove("X-Powered-By");
 
 await next();
});

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}

// Solo redirigir a HTTPS en desarrollo (en producción Render maneja HTTPS)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCookiePolicy();

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
