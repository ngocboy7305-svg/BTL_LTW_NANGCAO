using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;

namespace BTL_QLTuyenSinh.Controllers
{
    public class NganhHocsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NganhHocsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NganhHocs
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            // 1. Lưu trạng thái sắp xếp và bộ lọc vào ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["ScoreSortParm"] = sortOrder == "Score" ? "score_desc" : "Score";
            ViewData["CurrentFilter"] = searchString;

            // Khởi tạo truy vấn với AsNoTracking() để tối ưu hiệu năng
            var query = _context.NganhHoc.AsNoTracking().AsQueryable();

            // 2. SEARCH: Tìm kiếm theo Tên ngành hoặc Mã ngành (Where)
            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.TenNganh.Contains(searchString) || s.MaNganh.Contains(searchString));
            }

            // 3. SORT: Sắp xếp theo tên hoặc điểm sàn (OrderBy / OrderByDescending)
            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(s => s.TenNganh),
                "Score" => query.OrderBy(s => s.DiemSan),
                "score_desc" => query.OrderByDescending(s => s.DiemSan),
                _ => query.OrderBy(s => s.TenNganh)
            };

            // 4. PAGING: Phân trang (5 bản ghi / trang) (Skip / Take)
            int pageSize = 5;
            int page = pageNumber ?? 1;
            var totalRecords = await query.CountAsync();
            var pagedData = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return View(pagedData);
        }

        // GET: NganhHocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nganhHoc = await _context.NganhHoc
                .FirstOrDefaultAsync(m => m.MaNganhId == id);
            if (nganhHoc == null)
            {
                return NotFound();
            }

            return View(nganhHoc);
        }

        // GET: NganhHocs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NganhHocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNganhId,MaNganh,TenNganh,TenKhoa,ChiTieu,ToHopXetTuyen,DiemSan,DiemChuan,DangMoDangKy,DaXoa")] NganhHoc nganhHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nganhHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nganhHoc);
        }

        // GET: NganhHocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nganhHoc = await _context.NganhHoc.FindAsync(id);
            if (nganhHoc == null)
            {
                return NotFound();
            }
            return View(nganhHoc);
        }

        // POST: NganhHocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNganhId,MaNganh,TenNganh,TenKhoa,ChiTieu,ToHopXetTuyen,DiemSan,DiemChuan,DangMoDangKy,DaXoa")] NganhHoc nganhHoc)
        {
            if (id != nganhHoc.MaNganhId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nganhHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NganhHocExists(nganhHoc.MaNganhId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(nganhHoc);
        }

        // GET: NganhHocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nganhHoc = await _context.NganhHoc
                .FirstOrDefaultAsync(m => m.MaNganhId == id);
            if (nganhHoc == null)
            {
                return NotFound();
            }

            return View(nganhHoc);
        }

        // POST: NganhHocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nganhHoc = await _context.NganhHoc.FindAsync(id);
            if (nganhHoc != null)
            {
                _context.NganhHoc.Remove(nganhHoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NganhHocExists(int id)
        {
            return _context.NganhHoc.Any(e => e.MaNganhId == id);
        }
    }
}
