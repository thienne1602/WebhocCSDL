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
    }
}