using Microsoft.AspNetCore.Identity;

namespace BTL_QLTuyenSinh.Models.IdentityModels
{
    public class AppIdentityUser : IdentityUser
    {
        public string? HoTen { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}