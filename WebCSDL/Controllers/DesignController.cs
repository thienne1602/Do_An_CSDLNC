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
                    // Thêm class custom-table vào bảng
                    html += "<table class='table custom-table'>";
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
                    // Thêm class custom-table vào bảng
                    html += "<table class='table custom-table'>";
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

                // Phần 1: Kiểm tra chuẩn hóa (sẽ hiển thị trong tab 1)
                string checkNormalizationHtml = "<h4>Kiểm tra chuẩn hóa</h4>";
                if (entities == null || !entities.Any())
                {
                    checkNormalizationHtml += "<p>Chưa có thực thể nào để đánh giá.</p>";
                    return Content($"<div class='check-normalization'>{checkNormalizationHtml}</div>", "text/html");
                }

                // Kiểm tra 1NF: Mỗi thuộc tính phải là giá trị nguyên tử (atomic) và có khóa chính
                checkNormalizationHtml += "<h5>Kiểm tra 1NF (First Normal Form)</h5>";
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
                }

                if (is1NF)
                {
                    checkNormalizationHtml += "<p>Tất cả thực thể đều đạt 1NF.</p>";
                }
                else
                {
                    checkNormalizationHtml += "<ul>";
                    foreach (var issue in issues1NF)
                    {
                        checkNormalizationHtml += $"<li>{issue}</li>";
                    }
                    checkNormalizationHtml += "</ul>";
                }

                // Kiểm tra 2NF: Đạt 1NF và không có phụ thuộc hàm một phần (partial dependency)
                checkNormalizationHtml += "<h5>Kiểm tra 2NF (Second Normal Form)</h5>";
                bool is2NF = is1NF;
                List<string> issues2NF = new();
                foreach (var entity in entities)
                {
                    if (!is1NF) continue;
                    var primaryKeys = entity.Attributes?.Where(a => a.IsPrimaryKey).ToList();
                    var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                    if (primaryKeys != null && primaryKeys.Count > 1)
                    {
                        issues2NF.Add($"{entity.Name}: Có thể có phụ thuộc hàm một phần (giả định). Cần kiểm tra dữ liệu thực tế.");
                        is2NF = false;
                    }
                }

                if (is2NF)
                {
                    checkNormalizationHtml += "<p>Tất cả thực thể đều đạt 2NF.</p>";
                }
                else
                {
                    checkNormalizationHtml += "<ul>";
                    foreach (var issue in issues2NF)
                    {
                        checkNormalizationHtml += $"<li>{issue}</li>";
                    }
                    checkNormalizationHtml += "</ul>";
                }

                // Kiểm tra 3NF: Đạt 2NF và không có phụ thuộc bắc cầu (transitive dependency)
                checkNormalizationHtml += "<h5>Kiểm tra 3NF (Third Normal Form)</h5>";
                bool is3NF = is2NF;
                List<string> issues3NF = new();
                foreach (var entity in entities)
                {
                    if (!is2NF) continue;
                    var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                    if (nonPrimaryKeys != null && nonPrimaryKeys.Count > 1)
                    {
                        issues3NF.Add($"{entity.Name}: Có thể có phụ thuộc bắc cầu (giả định). Cần kiểm tra dữ liệu thực tế.");
                        is3NF = false;
                    }
                }

                if (is3NF)
                {
                    checkNormalizationHtml += "<p>Tất cả thực thể đều đạt 3NF.</p>";
                }
                else
                {
                    checkNormalizationHtml += "<ul>";
                    foreach (var issue in issues3NF)
                    {
                        checkNormalizationHtml += $"<li>{issue}</li>";
                    }
                    checkNormalizationHtml += "</ul>";
                }

                // Kiểm tra BCNF: Đạt 3NF và mọi phụ thuộc hàm X -> Y, thì X phải là siêu khóa
                checkNormalizationHtml += "<h5>Kiểm tra BCNF (Boyce-Codd Normal Form)</h5>";
                bool isBCNF = is3NF;
                List<string> issuesBCNF = new();
                foreach (var entity in entities)
                {
                    if (!is3NF) continue;
                    var primaryKeys = entity.Attributes?.Where(a => a.IsPrimaryKey).ToList();
                    var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                    if (nonPrimaryKeys != null && nonPrimaryKeys.Count > 0)
                    {
                        // Giả định có một phụ thuộc hàm mà bên trái không phải là siêu khóa
                        // Ví dụ: SinhVien(MaSV, MaLop, TenLop) có MaLop -> TenLop, nhưng MaLop không phải siêu khóa
                        issuesBCNF.Add($"{entity.Name}: Có thể có phụ thuộc hàm không thỏa BCNF (giả định). Cần kiểm tra dữ liệu thực tế.");
                        isBCNF = false;
                    }
                }

                if (isBCNF)
                {
                    checkNormalizationHtml += "<p>Tất cả thực thể đều đạt BCNF.</p>";
                }
                else
                {
                    checkNormalizationHtml += "<ul>";
                    foreach (var issue in issuesBCNF)
                    {
                        checkNormalizationHtml += $"<li>{issue}</li>";
                    }
                    checkNormalizationHtml += "</ul>";
                }

                // Kết luận dạng chuẩn cao nhất
                checkNormalizationHtml += "<h5>Kết luận dạng chuẩn cao nhất</h5>";
                if (isBCNF)
                {
                    checkNormalizationHtml += "<p>Lược đồ hiện tại đạt <strong>BCNF</strong> - dạng chuẩn cao nhất.</p>";
                }
                else if (is3NF)
                {
                    checkNormalizationHtml += "<p>Lược đồ hiện tại đạt <strong>3NF</strong>. Cần khắc phục để đạt BCNF.</p>";
                }
                else if (is2NF)
                {
                    checkNormalizationHtml += "<p>Lược đồ hiện tại đạt <strong>2NF</strong>. Cần khắc phục để đạt 3NF và BCNF.</p>";
                }
                else if (is1NF)
                {
                    checkNormalizationHtml += "<p>Lược đồ hiện tại đạt <strong>1NF</strong>. Cần khắc phục để đạt 2NF, 3NF và BCNF.</p>";
                }
                else
                {
                    checkNormalizationHtml += "<p>Lược đồ hiện tại <strong>không đạt 1NF</strong>. Cần khắc phục để đạt các dạng chuẩn cao hơn.</p>";
                }

                // Phần 2: Hướng giải quyết và mô hình chuẩn hóa (sẽ hiển thị trong tab 2)
                string solutionHtml = "<h4>Hướng giải quyết và mô hình chuẩn hóa</h4>";
                if (isBCNF)
                {
                    solutionHtml += "<p>Lược đồ đã đạt BCNF, không cần chuẩn hóa thêm.</p>";
                }
                else
                {
                    solutionHtml += "<h5>Các bước chuẩn hóa</h5>";
                    solutionHtml += "<p>Để đạt được dạng chuẩn cao nhất (BCNF), cần thực hiện các bước sau:</p>";
                    solutionHtml += "<ol>";

                    if (!is1NF)
                    {
                        solutionHtml += "<li><strong>Đạt 1NF:</strong> Đảm bảo mỗi thực thể có ít nhất một thuộc tính và một khóa chính. ";
                        solutionHtml += "<br><em>Chú thích:</em> 1NF yêu cầu tất cả thuộc tính phải là giá trị nguyên tử (atomic) và không có tập hợp lặp lại. ";
                        solutionHtml += "Ví dụ: Nếu thực thể SinhVien có thuộc tính 'DanhSachMonHoc' chứa nhiều giá trị (Mon1, Mon2, ...), cần tách thành bảng riêng.";
                        solutionHtml += "<br><em>Hành động đề xuất:</em> Thêm khóa chính cho các thực thể chưa có (ví dụ: MaSV cho SinhVien).";
                        solutionHtml += "</li>";
                    }

                    if (!is2NF)
                    {
                        solutionHtml += "<li><strong>Đạt 2NF:</strong> Loại bỏ phụ thuộc hàm một phần bằng cách tách các thuộc tính phụ thuộc vào một phần khóa chính thành bảng riêng. ";
                        solutionHtml += "<br><em>Chú thích:</em> 2NF áp dụng cho các thực thể có khóa chính ghép (nhiều hơn 1 thuộc tính). ";
                        solutionHtml += "Ví dụ: Nếu SinhVien(MaSV, MaLop, TenSV) với MaSV, MaLop là khóa chính ghép, và TenSV chỉ phụ thuộc vào MaSV, thì cần tách thành: ";
                        solutionHtml += "<ul><li>SinhVien(MaSV, TenSV)</li><li>LopHoc(MaSV, MaLop)</li></ul>";
                        solutionHtml += "<br><em>Hành động đề xuất:</em> Kiểm tra các thực thể có khóa chính ghép và tách các thuộc tính không phụ thuộc đầy đủ.";
                        solutionHtml += "</li>";
                    }

                    if (!is3NF)
                    {
                        solutionHtml += "<li><strong>Đạt 3NF:</strong> Loại bỏ phụ thuộc bắc cầu bằng cách tách các thuộc tính không khóa phụ thuộc vào thuộc tính không khóa khác. ";
                        solutionHtml += "<br><em>Chú thích:</em> Phụ thuộc bắc cầu xảy ra khi một thuộc tính không khóa phụ thuộc vào một thuộc tính không khóa khác. ";
                        solutionHtml += "Ví dụ: Nếu SinhVien(MaSV, MaLop, TenLop) với MaSV là khóa chính, và MaLop -> TenLop, thì cần tách thành: ";
                        solutionHtml += "<ul><li>SinhVien(MaSV, MaLop)</li><li>LopHoc(MaLop, TenLop)</li></ul>";
                        solutionHtml += "<br><em>Hành động đề xuất:</em> Xác định các phụ thuộc bắc cầu và tách thành bảng riêng.";
                        solutionHtml += "</li>";
                    }

                    if (!isBCNF)
                    {
                        solutionHtml += "<li><strong>Đạt BCNF:</strong> Đảm bảo mọi phụ thuộc hàm X -> Y, thì X phải là siêu khóa. ";
                        solutionHtml += "<br><em>Chú thích:</em> BCNF là dạng chuẩn nghiêm ngặt hơn 3NF, yêu cầu mọi phụ thuộc hàm đều phải có bên trái là siêu khóa. ";
                        solutionHtml += "Ví dụ: Nếu SinhVien(MaSV, MaLop, TenLop) với MaSV là khóa chính, và MaLop -> TenLop (MaLop không phải siêu khóa), thì cần tách thành: ";
                        solutionHtml += "<ul><li>SinhVien(MaSV, MaLop)</li><li>LopHoc(MaLop, TenLop)</li></ul>";
                        solutionHtml += "<br><em>Hành động đề xuất:</em> Xác định các phụ thuộc hàm không thỏa BCNF và tách thành bảng riêng.";
                        solutionHtml += "</li>";
                    }

                    solutionHtml += "</ol>";

                    // Mô hình chuẩn hóa (giả định đạt BCNF)
                    solutionHtml += "<h5>Mô hình chuẩn hóa (đạt BCNF)</h5>";
                    solutionHtml += "<p>Dựa trên các bước trên, lược đồ có thể được chuẩn hóa như sau:</p>";
                    solutionHtml += "<ul>";
                    foreach (var entity in entities)
                    {
                        var primaryKeys = entity.Attributes?.Where(a => a.IsPrimaryKey).ToList();
                        var nonPrimaryKeys = entity.Attributes?.Where(a => !a.IsPrimaryKey).ToList();
                        if (primaryKeys == null || primaryKeys.Count == 0 || nonPrimaryKeys == null) continue;

                        // Giả định tách bảng để đạt BCNF
                        solutionHtml += $"<li><strong>{entity.Name}({string.Join(", ", primaryKeys.Select(a => a.Name))}):</strong> {string.Join(", ", primaryKeys.Select(a => a.Name))}";
                        solutionHtml += "<br><em>Chú thích:</em> Đây là bảng chính chứa các khóa chính và các thuộc tính trực tiếp phụ thuộc vào khóa chính.";
                        solutionHtml += "</li>";

                        if (nonPrimaryKeys.Count > 0)
                        {
                            solutionHtml += $"<li><strong>{entity.Name}_Details({primaryKeys.First().Name}, {nonPrimaryKeys.First().Name}):</strong> {nonPrimaryKeys.First().Name}";
                            solutionHtml += "<br><em>Chú thích:</em> Tách các thuộc tính không khóa để loại bỏ phụ thuộc bắc cầu hoặc phụ thuộc không thỏa BCNF.";
                            solutionHtml += "</li>";
                        }
                    }
                    solutionHtml += "</ul>";
                }

                // Kết hợp hai phần vào một HTML với các div riêng biệt để hiển thị trong tab
                string combinedHtml = $"<div class='check-normalization'>{checkNormalizationHtml}</div>" +
                                     $"<div class='solution-normalization'>{solutionHtml}</div>";
                return Content(combinedHtml, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EvaluateSchema: {ex.Message}");
                return Content($"<div class='check-normalization'><p>Lỗi khi đánh giá lược đồ: {ex.Message}</p></div>", "text/html");
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


 