using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;
using Website_Cosmetics.Repositories;
using Website_Cosmetics.Data;
using Website_Cosmetics.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework - chỉ sử dụng ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions => sqlOptions.CommandTimeout(120))); // Tăng timeout lên 120 giây

// Add Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Add Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add RAG Service
builder.Services.AddScoped<IRAGService, RAGService>();

// Add HttpClient for VirtualMakeupService
builder.Services.AddHttpClient();

// Add VirtualMakeupService
builder.Services.AddScoped<VirtualMakeupService>();

// Add Session support (for chat session management)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed database - CHỈ seed khi database TRỐNG (không xóa dữ liệu cũ)
// Nếu muốn tắt hoàn toàn, comment đoạn code dưới đây
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with MIME types for AVIF and WebP
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
// Add AVIF if not already present
if (!contentTypeProvider.Mappings.ContainsKey(".avif"))
{
    contentTypeProvider.Mappings.Add(".avif", "image/avif");
}
// Add WebP if not already present (WebP might already be in default mappings)
if (!contentTypeProvider.Mappings.ContainsKey(".webp"))
{
    contentTypeProvider.Mappings.Add(".webp", "image/webp");
}

var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
};
app.UseStaticFiles(staticFileOptions);

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapAreaControllerRoute(
  name: "admin",
  areaName: "Admin",
  pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
