using CodersGear.DataAccess.Data;
using CodersGear.DataAccess.Repository;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using CodersGear.Utility;
using Stripe;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add API controllers for webhooks
builder.Services.AddControllers();

// Configure SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<PrintifySettings>(builder.Configuration.GetSection("Printify"));

// Register HttpClient for Printify service
builder.Services.AddHttpClient<IPrintifyService, PrintifyService>();
builder.Services.AddScoped<IPrintifyProductSyncService, PrintifyProductSyncService>();
builder.Services.AddScoped<IPrintifyOrderService, PrintifyOrderService>();

// Register webhook signature verifier for Printify HMAC-SHA256 validation
builder.Services.AddScoped<IWebhookSignatureVerifier, WebhookSignatureVerifier>();

// Register Printify background sync service
builder.Services.AddHostedService<PrintifyBackgroundSyncService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Add distributed memory cache for session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Session lasts 7 days
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Configure antiforgery to ignore Stripe webhook endpoint
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    // The webhook endpoint will be secured via Stripe signature verification instead
});

var app = builder.Build();

// Apply database migrations or create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // First try to apply migrations
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
            Console.WriteLine("Database migration completed successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        // Fallback: Ensure database is created (creates tables from current model)
        try
        {
            db.Database.EnsureCreated();
            Console.WriteLine("Database created/verified successfully using EnsureCreated.");
        }
        catch (Exception createEx)
        {
            Console.WriteLine($"Database creation also failed: {createEx.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure Stripe - handle missing key gracefully
var stripeKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
if (!string.IsNullOrEmpty(stripeKey))
{
    StripeConfiguration.ApiKey = stripeKey;
}
else
{
    Console.WriteLine("Warning: Stripe:SecretKey not configured");
}

app.UseRouting();
app.UseSession(); // Must be before UseAuthentication

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map API controllers (for Stripe webhook)
app.MapControllers();


app.Run();
