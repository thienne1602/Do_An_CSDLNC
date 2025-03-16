using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCSDL.Data;
using WebCSDL.Models;

namespace WebCSDL.Controllers
{
    public class DesignController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Conceptual()
        {
            try
            {
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .ToListAsync();
                var relationships = await _context.Relationships.ToListAsync();

                string html = "<h4>Thiết kế quan niệm</h4>";
                html += "<h5>Thực thể</h5>";
                if (entities == null || !entities.Any())
                {
                    html += "<p>Chưa có thực thể nào.</p>";
                }
                else
                {
                    html += "<ul>";
                    foreach (var entity in entities)
                    {
                        html += $"<li>{entity.Name}: ";
                        if (entity.Attributes != null && entity.Attributes.Any())
                        {
                            html += string.Join(", ", entity.Attributes.Select(a => $"{a.Name}{(a.IsPrimaryKey ? " (PK)" : "")}"));
                        }
                        else
                        {
                            html += "Chưa có thuộc tính";
                        }
                        html += "</li>";
                    }
                    html += "</ul>";
                }

                html += "<h5>Mối quan hệ</h5>";
                if (relationships == null || !relationships.Any())
                {
                    html += "<p>Chưa có mối quan hệ nào.</p>";
                }
                else
                {
                    html += "<ul>";
                    foreach (var rel in relationships)
                    {
                        html += $"<li>{rel.Entity1} {rel.Type} {rel.Entity2}</li>";
                    }
                    html += "</ul>";
                }

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Conceptual: {ex.Message}");
                return Content($"<p>Lỗi khi tải thiết kế quan niệm: {ex.Message}</p>", "text/html");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logical()
        {
            try
            {
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .ToListAsync();
                var relationships = await _context.Relationships.ToListAsync();

                string html = "<h4>Cấu trúc logic</h4>";
                html += "<h5>Thực thể</h5>";
                if (entities == null || !entities.Any())
                {
                    html += "<p>Chưa có thực thể nào.</p>";
                }
                else
                {
                    html += "<table class='table'>";
                    html += "<thead><tr><th>Tên thực thể</th><th>Thuộc tính</th><th>Khóa chính</th></tr></thead>";
                    html += "<tbody>";
                    foreach (var entity in entities)
                    {
                        if (entity.Attributes != null && entity.Attributes.Any())
                        {
                            foreach (var attr in entity.Attributes)
                            {
                                html += "<tr>";
                                html += $"<td>{entity.Name}</td>";
                                html += $"<td>{attr.Name}</td>";
                                html += $"<td>{(attr.IsPrimaryKey ? "Yes" : "No")}</td>";
                                html += "</tr>";
                            }
                        }
                        else
                        {
                            html += "<tr>";
                            html += $"<td>{entity.Name}</td>";
                            html += "<td colspan='2'>Chưa có thuộc tính</td>";
                            html += "</tr>";
                        }
                    }
                    html += "</tbody></table>";
                }

                html += "<h5>Mối quan hệ</h5>";
                if (relationships == null || !relationships.Any())
                {
                    html += "<p>Chưa có mối quan hệ nào.</p>";
                }
                else
                {
                    html += "<table class='table'>";
                    html += "<thead><tr><th>Thực thể 1</th><th>Thực thể 2</th><th>Loại</th></tr></thead>";
                    html += "<tbody>";
                    foreach (var rel in relationships)
                    {
                        html += "<tr>";
                        html += $"<td>{rel.Entity1}</td>";
                        html += $"<td>{rel.Entity2}</td>";
                        html += $"<td>{rel.Type}</td>";
                        html += "</tr>";
                    }
                    html += "</tbody></table>";
                }

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Logical: {ex.Message}");
                return Content($"<p>Lỗi khi tải cấu trúc logic: {ex.Message}</p>", "text/html");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EvaluateSchema()
        {
            try
            {
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .ToListAsync();
                var relationships = await _context.Relationships.ToListAsync();

                string html = "<h4>Đánh giá và chuẩn hóa lược đồ</h4>";

                // Kiểm tra chuẩn hóa
                html += "<h5>Kiểm tra chuẩn hóa</h5>";
                if (entities == null || !entities.Any())
                {
                    html += "<p>Chưa có thực thể nào để đánh giá.</p>";
                    return Content(html, "text/html");
                }

                // Kiểm tra 1NF: Mỗi thuộc tính phải là giá trị nguyên tử (atomic) và có khóa chính
                html += "<h6>Kiểm tra 1NF (First Normal Form)</h6>";
                bool is1NF = true;
                List<string> issues1NF = new();
                foreach (var entity in entities)
                {
                    if (entity.Attributes == null || !entity.Attributes.Any())
                    {
                        issues1NF.Add($"{entity.Name}: Không có thuộc tính.");
                        is1NF = false;
                    }
                    else if (!entity.Attributes.Any(a => a.IsPrimaryKey))
                    {
                        issues1NF.Add($"{entity.Name}: Không có khóa chính.");
                        is1NF = false;
                    }
                    // Giả sử các thuộc tính đều là nguyên tử (vì không có cách kiểm tra dữ liệu thực tế)
                }

                if (is1NF)
                {
                    html += "<p>Tất cả thực thể đều đạt 1NF.</p>";
                }
                else
                {
                    html += "<ul>";
                    foreach (var issue in issues1NF)
                    {
                        html += $"<li>{issue}</li>";
                    }
                    html += "</ul>";
                }

                // Kiểm tra 2NF: Đạt 1NF và không có phụ thuộc hàm một phần (partial dependency)
                html += "<h6>Kiểm tra 2NF (Second Normal Form)</h6>";
                bool is2NF = is1NF; // Đạt 2NF nếu đã đạt 1NF
                List<string> issues2NF = new();
                foreach (var entity in entities)
                {
                    if (!is1NF) continue; // Bỏ qua nếu không đạt 1NF
                    var primaryKeys = entity.Attributes?.Where(a => a.IsPrimaryKey).ToList();
                    var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                    if (primaryKeys != null && primaryKeys.Count > 1) // Nếu có khóa chính ghép
                    {
                        // Giả sử một số thuộc tính phụ thuộc vào một phần của khóa chính (ví dụ: SinhVien(MaSV, MaLop) -> TenSV phụ thuộc chỉ vào MaSV)
                        issues2NF.Add($"{entity.Name}: Có thể có phụ thuộc hàm một phần (giả định). Cần kiểm tra dữ liệu thực tế.");
                        is2NF = false;
                    }
                }

                if (is2NF)
                {
                    html += "<p>Tất cả thực thể đều đạt 2NF.</p>";
                }
                else
                {
                    html += "<ul>";
                    foreach (var issue in issues2NF)
                    {
                        html += $"<li>{issue}</li>";
                    }
                    html += "</ul>";
                }

                // Kiểm tra 3NF: Đạt 2NF và không có phụ thuộc bắc cầu (transitive dependency)
                html += "<h6>Kiểm tra 3NF (Third Normal Form)</h6>";
                bool is3NF = is2NF; // Đạt 3NF nếu đã đạt 2NF
                List<string> issues3NF = new();
                foreach (var entity in entities)
                {
                    if (!is2NF) continue; // Bỏ qua nếu không đạt 2NF
                                          // Giả sử một số thuộc tính không khóa phụ thuộc vào thuộc tính không khóa khác (ví dụ: SinhVien(MaSV, MaLop) -> TenLop phụ thuộc vào MaLop)
                    var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                    if (nonPrimaryKeys != null && nonPrimaryKeys.Count > 1)
                    {
                        issues3NF.Add($"{entity.Name}: Có thể có phụ thuộc bắc cầu (giả định). Cần kiểm tra dữ liệu thực tế.");
                        is3NF = false;
                    }
                }

                if (is3NF)
                {
                    html += "<p>Tất cả thực thể đều đạt 3NF.</p>";
                }
                else
                {
                    html += "<ul>";
                    foreach (var issue in issues3NF)
                    {
                        html += $"<li>{issue}</li>";
                    }
                    html += "</ul>";
                }

                // Đề xuất hướng giải quyết và mô hình chuẩn hóa
                html += "<h5>Hướng giải quyết và mô hình chuẩn hóa</h5>";
                if (is3NF)
                {
                    html += "<p>Lược đồ đã đạt 3NF, không cần chuẩn hóa thêm.</p>";
                }
                else
                {
                    html += "<p>Các bước chuẩn hóa:</p>";
                    html += "<ol>";
                    if (!is1NF)
                    {
                        html += "<li>Đảm bảo 1NF: Thêm khóa chính và đảm bảo tất cả thuộc tính là nguyên tử.</li>";
                    }
                    if (!is2NF)
                    {
                        html += "<li>Đạt 2NF: Tách các thuộc tính phụ thuộc một phần thành bảng riêng. Ví dụ: Nếu SinhVien(MaSV, MaLop) có TenSV phụ thuộc chỉ vào MaSV, tách thành SinhVien(MaSV, TenSV) và LopHoc(MaSV, MaLop).</li>";
                    }
                    if (!is3NF)
                    {
                        html += "<li>Đạt 3NF: Tách các thuộc tính có phụ thuộc bắc cầu. Ví dụ: Nếu SinhVien(MaSV, MaLop, TenLop) thì tách thành SinhVien(MaSV, MaLop) và LopHoc(MaLop, TenLop).</li>";
                    }
                    html += "</ol>";

                    // Mô hình chuẩn hóa (giả định)
                    html += "<h6>Mô hình chuẩn hóa (đạt 3NF)</h6>";
                    html += "<p>Dựa trên giả định, lược đồ có thể được chuẩn hóa như sau:</p>";
                    html += "<ul>";
                    foreach (var entity in entities)
                    {
                        var primaryKeys = entity.Attributes?.Where(a => a.IsPrimaryKey).ToList();
                        var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                        if (primaryKeys == null || primaryKeys.Count == 0 || nonPrimaryKeys == null) continue;

                        // Giả sử tách bảng để đạt 3NF
                        html += $"<li>{entity.Name}({string.Join(", ", primaryKeys.Select(a => a.Name))}): {string.Join(", ", primaryKeys.Select(a => a.Name))}</li>";
                        if (nonPrimaryKeys.Count > 1)
                        {
                            html += $"<li>{entity.Name}_Details({primaryKeys.First().Name}, {nonPrimaryKeys.First().Name}): {nonPrimaryKeys.First().Name}</li>";
                        }
                    }
                    html += "</ul>";
                }

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EvaluateSchema: {ex.Message}");
                return Content($"<p>Lỗi khi đánh giá lược đồ: {ex.Message}</p>", "text/html");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateERD()
        {
            try
            {
                // Lấy danh sách thực thể và thuộc tính
                var entities = await _context.Entities
                    .Include(e => e.Attributes)
                    .ToListAsync();

                // Lấy danh sách mối quan hệ
                var relationships = await _context.Relationships.ToListAsync();

                // Tạo mã erDiagram
                string mermaidCode = "erDiagram\n";

                // Thêm các thực thể và thuộc tính
                if (entities == null || !entities.Any())
                {
                    mermaidCode += "    Chưa có thực thể nào để tạo ERD.\n";
                }
                else
                {
                    foreach (var entity in entities)
                    {
                        mermaidCode += $"    {entity.Name} {{\n";
                        if (entity.Attributes != null && entity.Attributes.Any())
                        {
                            foreach (var attr in entity.Attributes)
                            {
                                string dataType = attr.DataType ?? "nvarchar"; // Mặc định là nvarchar nếu không có DataType
                                string pkIndicator = attr.IsPrimaryKey ? "PK" : "";
                                mermaidCode += $"        {dataType} {attr.Name} {pkIndicator}\n";
                            }
                        }
                        else
                        {
                            mermaidCode += "        Chưa có thuộc tính\n";
                        }
                        mermaidCode += "    }\n";
                    }
                }

                // Thêm các mối quan hệ
                if (relationships != null && relationships.Any())
                {
                    foreach (var rel in relationships)
                    {
                        string cardinality = "";
                        switch (rel.Type)
                        {
                            case "1-1":
                                cardinality = "||..||";
                                break;
                            case "1-N":
                                cardinality = "||--o{";
                                break;
                            case "N-M":
                                cardinality = "}o--o{";
                                break;
                            default:
                                cardinality = "--";
                                break;
                        }
                        mermaidCode += $"    {rel.Entity1} {cardinality} {rel.Entity2} : \"{rel.Type}\"\n";
                    }
                }

                return Json(new { success = true, mermaidCode = mermaidCode });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateERD: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}


 