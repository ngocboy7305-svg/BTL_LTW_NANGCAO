using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

// 1. Cấu hình DbContext kết nối SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Cookie Authentication (Bắt buộc cho FR-01 & FR-08)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";              // Chuyển hướng khi chưa đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Chuyển hướng khi không đủ quyền (403)
        options.ExpireTimeSpan = TimeSpan.FromHours(8);      // Phiên đăng nhập tồn tại 8 tiếng
        options.SlidingExpiration = true;                   // Tự động gia hạn khi có thao tác
    });

builder.Services.AddAuthorization();

// 3. Đăng ký Service Xét tuyển tự động (Bắt buộc cho FR-12 & FR-13)
builder.Services.AddScoped<XetTuyenService>();

// 4. Thêm MVC Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Khởi tạo Seed Data ban đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra khi khởi tạo Seed Data.");
    }
}

// 6. Cấu hình Pipeline xử lý HTTP request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// THỨ TỰ BẮT BUỘC: Authentication phải đứng TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Route cho khu vực Admin (Quản lý người dùng, Xét tuyển)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 8. Route mặc định (Thí sinh, Trang chủ, Đăng nhập)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();