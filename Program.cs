using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ----------------- DB AYARI -----------------
// appsettings.json içindeki "DefaultConnection" okunur.
// Boşsa fallback olarak SQLite "app.db" kullanır.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // SQL Server kullanmak istersen:
    // options.UseSqlServer(connectionString);

    // Şu an elimizde app.db olduğu için SQLite mantıklı:
    options.UseSqlite(connectionString);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ----------------- IDENTITY AYARI -----------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Şifreyi biraz esnek yapalım:
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie ayarı (login sayfası vs)
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Identity/Account/Login";
    opt.LogoutPath = "/Identity/Account/Logout";
    opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ----------------- KÜLTÜR (TR) -----------------
var supportedCultures = new[] { new CultureInfo("tr-TR") };

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// ----------------- HTTP PIPELINE -----------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Varsayılan rota: / -> Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity Razor Pages
app.MapRazorPages();

// ----------------- UYGULAMAYI ÇALIŞTIR -----------------
app.Run();

