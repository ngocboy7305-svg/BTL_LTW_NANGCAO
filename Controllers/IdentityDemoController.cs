using BTL_QLTuyenSinh.Models.IdentityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTL_QLTuyenSinh.Controllers
{
    public class IdentityDemoController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityDemoController(
            UserManager<AppIdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // URL: /IdentityDemo/TestIdentity
        [HttpGet]
        public async Task<IActionResult> TestIdentity()
        {
            // 1. Tạo Role thử nghiệm nếu chưa có
            if (!await _roleManager.RoleExistsAsync("CanBoTuyenSinh"))
            {
                await _roleManager.CreateAsync(new IdentityRole("CanBoTuyenSinh"));
            }

            // 2. Tạo tài khoản thử nghiệm qua Identity Framework
            var email = "canbo_identity@tuyensinh.edu.vn";
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppIdentityUser
                {
                    UserName = email,
                    Email = email,
                    HoTen = "Cán Bộ Thử Nghiệm Identity"
                };

                // Identity tự động mã hóa mật khẩu theo thuật toán PBKDF2
                var result = await _userManager.CreateAsync(user, "123456@Aa");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "CanBoTuyenSinh");
                    return Content($"[Thành công] Đã tạo tài khoản {email} với Role 'CanBoTuyenSinh' bằng Identity Framework!");
                }

                return Content("Lỗi tạo user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Content($"[Thông báo] Tài khoản {email} đã tồn tại trong bảng AspNetUsers.");
        }
    }
}