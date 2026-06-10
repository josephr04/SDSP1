using AspNetCoreRateLimit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

using SDSP1.Database;
using SDSP1.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Conexion>();
builder.Services.AddScoped<RegistrarService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<CarpetasService>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<TotpService>();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"))
    .SetApplicationName("SDSP1"); builder.Services.AddMemoryCache();
builder.Services.AddScoped<RecuperacionService>();
builder.Services.AddHttpClient<EmailService>(); 
builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// needed to load configuration from appsettings.json
builder.Services.AddOptions();

// needed to store rate limit counters and ip rules
builder.Services.AddMemoryCache();

//load general configuration from appsettings.json
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

//load ip rules from appsettings.json
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

// inject counter and rules stores
builder.Services.AddInMemoryRateLimiting();
//services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
//services.AddDistributedRateLimiting<RedisProcessingStrategy>();
//services.AddRedisRateLimiting();

// configuration (resolvers, counter key builders)
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Login/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

