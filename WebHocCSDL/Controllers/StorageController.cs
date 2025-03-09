using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocCSDL.Data;

namespace WebHocCSDL.Controllers
{
    public class StorageController : Controller
    {
        private readonly AppDbContext _context;

        public StorageController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var designs = await _context.Designs.ToListAsync();
            return View(designs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null)
            {
                return NotFound();
            }
            return View(design);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null)
            {
                return NotFound();
            }

            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Quay lại danh sách sau khi xoá
        }



    }
}