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
builder.Services.AddDbContext<ApplicationDbContext>(options=>
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();

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
