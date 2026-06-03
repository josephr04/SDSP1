using Microsoft.AspNetCore.Identity;

using SDSP1.Database;
using SDSP1.Services;

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
builder.Services.AddScoped<RecuperacionService>();
builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

