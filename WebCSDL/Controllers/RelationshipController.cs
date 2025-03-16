using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebCSDL.Data;
using WebCSDL.Models;

namespace WebCSDL.Controllers
{
    public class RelationshipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RelationshipController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var relationships = await _context.Relationships.ToListAsync();
            return PartialView("_RelationshipList", relationships);
        }

        [HttpPost]
        public async Task<IActionResult> AddRelationship(string entity1, string entity2, string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity1) || string.IsNullOrWhiteSpace(entity2))
                {
                    return Json(new { success = false, message = "Tên thực thể không được để trống!" });
                }

                var entity1Exists = await _context.Entities.AnyAsync(e => e.Name == entity1);
                var entity2Exists = await _context.Entities.AnyAsync(e => e.Name == entity2);
                if (!entity1Exists || !entity2Exists)
                {
                    return Json(new { success = false, message = "Một trong hai thực thể không tồn tại!" });
                }

                if (await _context.Relationships.AnyAsync(r => r.Entity1 == entity1 && r.Entity2 == entity2))
                {
                    return Json(new { success = false, message = "Mối quan hệ đã tồn tại!" });
                }

                var relationship = new Relationship { Entity1 = entity1, Entity2 = entity2, Type = type };
                _context.Relationships.Add(relationship);
                await _context.SaveChangesAsync();

                var relationships = await _context.Relationships.ToListAsync();
                return PartialView("_RelationshipList", relationships);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi thêm mối quan hệ: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRelationship(string entity1, string entity2)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entity1) || string.IsNullOrWhiteSpace(entity2))
                {
                    return Json(new { success = false, message = "Tên thực thể không được để trống!" });
                }

                var relationship = await _context.Relationships
                    .FirstOrDefaultAsync(r => r.Entity1 == entity1 && r.Entity2 == entity2);
                if (relationship == null)
                {
                    return Json(new { success = false, message = "Mối quan hệ không tồn tại!" });
                }

                _context.Relationships.Remove(relationship);
                await _context.SaveChangesAsync();

                var relationships = await _context.Relationships.ToListAsync();
                return PartialView("_RelationshipList", relationships);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa mối quan hệ: {ex.Message}" });
            }
        }
    }
}