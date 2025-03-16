using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebCSDL.Data;
using WebCSDL.Models;

namespace WebCSDL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new HomeViewModel
                {
                    Entities = await _context.Entities.Include(e => e.Attributes).ToListAsync(),
                    Relationships = await _context.Relationships.ToListAsync()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" (Inner: {ex.InnerException.Message})";
                }
                Console.WriteLine(errorMessage);
                return StatusCode(500, errorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEntity(string name, string[] attributeNames, bool[] isPrimaryKeys)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, message = "Tên thực thể không được để trống!" });
                }

                if (attributeNames == null || attributeNames.Length == 0 || attributeNames.Any(an => string.IsNullOrWhiteSpace(an)))
                {
                    return Json(new { success = false, message = "Vui lòng thêm ít nhất một thuộc tính hợp lệ!" });
                }

                if (await _context.Entities.AnyAsync(e => e.Name == name))
                {
                    return Json(new { success = false, message = "Thực thể đã tồn tại!" });
                }

                // Bước 1: Tạo và lưu Entity trước để lấy Id
                var entity = new Entity { Name = name };
                _context.Entities.Add(entity);
                await _context.SaveChangesAsync(); // Lưu để sinh Id

                // Bước 2: Thêm các EntityAttribute với EntityId hợp lệ
                var attributes = new List<EntityAttribute>();
                for (int i = 0; i < attributeNames.Length; i++)
                {
                    var attribute = new EntityAttribute
                    {
                        Name = attributeNames[i],
                        IsPrimaryKey = isPrimaryKeys?[i] ?? false,
                        EntityId = entity.Id // Sử dụng Id đã được sinh
                    };
                    attributes.Add(attribute);
                }
                _context.EntityAttributes.AddRange(attributes);
                await _context.SaveChangesAsync(); // Lưu tất cả thuộc tính cùng lúc

                // Bước 3: Trả về danh sách thực thể cập nhật
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        attributes = e.Attributes.Select(a => new
                        {
                            name = a.Name,
                            isPrimaryKey = a.IsPrimaryKey,
                            dataType = a.DataType
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, entities = entities });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddEntity: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEntity(string name)
        {
            try
            {
                var entity = await _context.Entities
                    .Include(e => e.Attributes)
                    .FirstOrDefaultAsync(e => e.Name == name);

                if (entity == null)
                {
                    return Json(new { success = false, message = "Thực thể không tồn tại!" });
                }

                // Xóa các mối quan hệ liên quan
                var relationships = await _context.Relationships
                    .Where(r => r.Entity1 == name || r.Entity2 == name)
                    .ToListAsync();
                if (relationships.Any())
                {
                    _context.Relationships.RemoveRange(relationships);
                }

                // Xóa các thuộc tính liên quan
                if (entity.Attributes != null && entity.Attributes.Any())
                {
                    _context.EntityAttributes.RemoveRange(entity.Attributes);
                }

                // Xóa thực thể
                _context.Entities.Remove(entity);
                await _context.SaveChangesAsync();

                // Lấy lại danh sách thực thể và mối quan hệ
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        attributes = e.Attributes.Select(a => new
                        {
                            name = a.Name,
                            isPrimaryKey = a.IsPrimaryKey,
                            dataType = a.DataType
                        }).ToList()
                    })
                    .ToListAsync();

                var updatedRelationships = await _context.Relationships.ToListAsync();

                return Json(new { success = true, entities = entities, relationships = updatedRelationships });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteEntity: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditEntity(string oldName, string newName, string[] attributeNames, bool[] isPrimaryKeys)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                {
                    return Json(new { success = false, message = "Tên thực thể không được để trống!" });
                }

                if (attributeNames == null || attributeNames.Length == 0 || attributeNames.Any(an => string.IsNullOrWhiteSpace(an)))
                {
                    return Json(new { success = false, message = "Vui lòng thêm ít nhất một thuộc tính hợp lệ!" });
                }

                // Tải thực thể
                var entity = await _context.Entities
                    .Include(e => e.Attributes)
                    .FirstOrDefaultAsync(e => e.Name == oldName);

                if (entity == null)
                {
                    return Json(new { success = false, message = "Thực thể không tồn tại!" });
                }

                // Cập nhật tên thực thể
                entity.Name = newName;

                // Cập nhật Relationships nếu tên thay đổi
                var relationships = await _context.Relationships
                    .Where(r => r.Entity1 == oldName || r.Entity2 == oldName)
                    .ToListAsync();
                foreach (var rel in relationships)
                {
                    if (rel.Entity1 == oldName) rel.Entity1 = newName;
                    if (rel.Entity2 == oldName) rel.Entity2 = newName;
                }

                // Xóa các thuộc tính cũ
                if (entity.Attributes != null && entity.Attributes.Any())
                {
                    _context.EntityAttributes.RemoveRange(entity.Attributes);
                }

                // Thêm các thuộc tính mới
                var newAttributes = new List<EntityAttribute>();
                for (int i = 0; i < attributeNames.Length; i++)
                {
                    var attribute = new EntityAttribute
                    {
                        Name = attributeNames[i],
                        IsPrimaryKey = isPrimaryKeys?[i] ?? false,
                        EntityId = entity.Id
                    };
                    newAttributes.Add(attribute);
                }
                _context.EntityAttributes.AddRange(newAttributes);

                // Lưu thay đổi và kiểm tra concurrency
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Kiểm tra xem thực thể còn tồn tại không
                    var entry = ex.Entries.First();
                    if (!await _context.Entities.AnyAsync(e => e.Id == entity.Id))
                    {
                        return Json(new { success = false, message = "Thực thể đã bị xóa bởi người dùng khác!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại." });
                    }
                }

                // Lấy lại danh sách thực thể
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        attributes = e.Attributes.Select(a => new
                        {
                            name = a.Name,
                            isPrimaryKey = a.IsPrimaryKey,
                            dataType = a.DataType
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, entities = entities });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EditEntity: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
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

                var updatedRelationships = await _context.Relationships.ToListAsync();
                return Json(new { success = true, relationships = updatedRelationships });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in AddRelationship: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
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

                var updatedRelationships = await _context.Relationships.ToListAsync();
                return Json(new { success = true, relationships = updatedRelationships });
            }
            catch (Exception ex)
            {
                var errorMessage = $"Lỗi khi xóa mối quan hệ: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" (Inner: {ex.InnerException.Message})";
                }
                Console.WriteLine(errorMessage);
                return Json(new { success = false, message = errorMessage });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPhysicalDesign()
        {
            try
            {
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        attributes = e.Attributes.Select(a => new
                        {
                            name = a.Name,
                            isPrimaryKey = a.IsPrimaryKey,
                            dataType = a.DataType
                        }).ToList()
                    })
                    .ToListAsync();

                if (entities == null || !entities.Any())
                {
                    return Json(new { success = true, entities = new List<object>() });
                }

                return Json(new { success = true, entities = entities });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPhysicalDesign: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhysicalDesign(List<UpdateDataTypeModel> updates)
        {
            try
            {
                if (updates == null || !updates.Any())
                {
                    return Json(new { success = false, message = "Không có thay đổi nào để lưu!" });
                }

                foreach (var update in updates)
                {
                    var entity = await _context.Entities
                        .Include(e => e.Attributes)
                        .FirstOrDefaultAsync(e => e.Name == update.EntityName);

                    if (entity == null)
                    {
                        return Json(new { success = false, message = $"Thực thể {update.EntityName} không tồn tại!" });
                    }

                    var attribute = entity.Attributes?.FirstOrDefault(a => a.Name == update.AttributeName);
                    if (attribute == null)
                    {
                        return Json(new { success = false, message = $"Thuộc tính {update.AttributeName} của thực thể {update.EntityName} không tồn tại!" });
                    }

                    attribute.DataType = update.DataType;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.First();
                    return Json(new { success = false, message = "Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại." });
                }

                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Name,
                        attributes = e.Attributes.Select(a => new
                        {
                            name = a.Name,
                            isPrimaryKey = a.IsPrimaryKey,
                            dataType = a.DataType
                        }).ToList()
                    })
                    .ToListAsync();

                return Json(new { success = true, entities = entities });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePhysicalDesign: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        // Mô hình tạm thời để nhận dữ liệu từ client
        public class PhysicalDesignUpdateModel
        {
            public string? EntityName { get; set; }
            public string? AttributeName { get; set; }
            public string? DataType { get; set; }
        }
    }
}