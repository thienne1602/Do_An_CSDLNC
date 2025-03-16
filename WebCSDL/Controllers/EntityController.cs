using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebCSDL.Data;
using WebCSDL.Models;

namespace WebCSDL.Controllers
{
    public class EntityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entities = await _context.Entities.Include(e => e.Attributes).ToListAsync();
            return PartialView("_EntityList", entities);
        }

        [HttpPost]
        public async Task<IActionResult> AddEntity(string name, List<string> attributeNames, List<bool>? isPrimaryKeys)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return Json(new { success = false, message = "Tên thực thể không được để trống!" });
                }

                if (await _context.Entities.AnyAsync(e => e.Name == name))
                {
                    return Json(new { success = false, message = "Thực thể đã tồn tại!" });
                }

                var entity = new Entity { Name = name };
                for (int i = 0; i < attributeNames.Count; i++)
                {
                    bool isPrimaryKey = (isPrimaryKeys != null && i < isPrimaryKeys.Count) ? isPrimaryKeys[i] : false;
                    entity.Attributes.Add(new EntityAttribute { Name = attributeNames[i], IsPrimaryKey = isPrimaryKey });
                }

                _context.Entities.Add(entity);
                await _context.SaveChangesAsync();
                return PartialView("_EntityList", await _context.Entities.Include(e => e.Attributes).ToListAsync());
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi thêm thực thể: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEntity(string name)
        {
            var entity = await _context.Entities.Include(e => e.Attributes).FirstOrDefaultAsync(e => e.Name == name);
            if (entity != null)
            {
                _context.Entities.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return PartialView("_EntityList", await _context.Entities.Include(e => e.Attributes).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> EditEntity(string oldName, string newName, List<string> attributeNames, List<bool>? isPrimaryKeys)
        {
            var entity = await _context.Entities.Include(e => e.Attributes).FirstOrDefaultAsync(e => e.Name == oldName);
            if (entity != null)
            {
                entity.Name = newName;
                entity.Attributes.Clear();
                for (int i = 0; i < attributeNames.Count; i++)
                {
                    bool isPrimaryKey = (isPrimaryKeys != null && i < isPrimaryKeys.Count) ? isPrimaryKeys[i] : false;
                    entity.Attributes.Add(new EntityAttribute { Name = attributeNames[i], IsPrimaryKey = isPrimaryKey });
                }
                await _context.SaveChangesAsync();
            }
            return PartialView("_EntityList", await _context.Entities.Include(e => e.Attributes).ToListAsync());
        }
    }
}