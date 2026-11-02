using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models.IdentityModels;
using BTL_QLTuyenSinh.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext kết nối SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Cache & Session State (Yêu cầu Thực hành Buổi 7)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BTL_QLTuyenSinh.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tự hủy phiên sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;                 // Chống tấn công XSS đọc session
    options.Cookie.IsEssential = true;
});

// 3. Cấu hình Cookie Authentication (Phân quyền & Xác thực danh tính)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".BTL_QLTuyenSinh.Auth";
        options.LoginPath = "/Account/Login";               // Chuyển hướng khi chưa đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Chuyển hướng khi không đủ quyền (403)
        options.SlidingExpiration = true;                    // Gia hạn cookie khi người dùng thao tác
        options.ExpireTimeSpan = TimeSpan.FromHours(8);       // Thời gian tồn tại khi chọn ghi nhớ
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
// 1. Đăng ký AppIdentityDbContext (nếu chưa có)
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình đầy đủ Identity với AppIdentityUser và IdentityRole
builder.Services.AddIdentity<AppIdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddAuthorization();

// 4. Đăng ký Service Xét tuyển tự động (FR-12 & FR-13)
builder.Services.AddScoped<XetTuyenService>();
// 1. Cấu hình DbContext kết nối SQL Server (BTL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// THÊM DÒNG NÀY: Đăng ký AppIdentityDbContext cho module Identity (Buổi 7)
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 5. Thêm MVC Controllers with Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Khởi tạo Seed Data ban đầu
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

// 7. Cấu hình Pipeline xử lý HTTP Request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// THỨ TỰ BẮT BUỘC: Authentication -> Authorization -> Session
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// 8. Cấu hình định tuyến (Routes)
// Route dành cho khu vực Admin (Quản lý người dùng, Ngành học, Xét tuyển)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route mặc định cho Thí sinh & Khách
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();