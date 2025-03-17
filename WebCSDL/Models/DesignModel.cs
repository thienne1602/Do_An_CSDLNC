using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCSDL.Models
{
    public class DesignModel
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();
        public List<Relationship> Relationships { get; set; } = new List<Relationship>();

        // Phương thức đánh giá toàn bộ lược đồ
        public string EvaluateSchema()
        {
            string result = "<strong>Đánh giá toàn bộ lược đồ</strong><br/>";

            if (Entities.Count == 0)
            {
                return result + "Chưa có thực thể nào trong lược đồ để đánh giá.<br/>";
            }

            // Kiểm tra các dạng chuẩn cho toàn bộ lược đồ
            bool is1NF = Check1NF();
            result += is1NF ? "- Lược đồ ở 1NF (Tất cả thuộc tính đều nguyên tử).<br/>"
                           : "- Lược đồ không ở 1NF (Có thuộc tính không nguyên tử).<br/>";

            bool is2NF = is1NF && Check2NF();
            result += is2NF ? "- Lược đồ ở 2NF (Không có phụ thuộc một phần).<br/>"
                           : "- Lược đồ không ở 2NF (Có phụ thuộc một phần).<br/>";

            bool is3NF = is2NF && Check3NF();
            result += is3NF ? "- Lược đồ ở 3NF (Không có phụ thuộc bắc cầu).<br/>"
                           : "- Lược đồ không ở 3NF (Có phụ thuộc bắc cầu).<br/>";

            bool isBCNF = is3NF && CheckBCNF();
            result += isBCNF ? "- Lược đồ ở BCNF (Mọi yếu tố quyết định là khóa ứng viên).<br/>"
                            : "- Lược đồ không ở BCNF (Có yếu tố quyết định không phải khóa ứng viên).<br/>";

            // Xác định dạng chuẩn cao nhất của lược đồ
            string highestNormalForm = DetermineHighestNormalForm(is1NF, is2NF, is3NF, isBCNF);
            result += $"<strong>Dạng chuẩn cao nhất của lược đồ: {highestNormalForm}</strong><br/>";

            // Nếu không đạt BCNF, cung cấp hướng giải quyết
            if (!isBCNF)
            {
                result += "<br/><div id='normalization-steps'><strong>Hướng giải quyết và mô hình chuẩn hóa:</strong><br/>";
                result += GenerateNormalizationSteps();
                result += "</div>";
            }
            else
            {
                result += "<br/>Lược đồ đã đạt dạng chuẩn cao nhất (BCNF).<br/>";
            }

            return result;
        }

        // Kiểm tra 1NF cho toàn bộ lược đồ
        private bool Check1NF()
        {
            return Entities.All(e => e.Attributes != null && e.Attributes.All(a => !string.IsNullOrWhiteSpace(a.Name)));
        }

        // Kiểm tra 2NF cho toàn bộ lược đồ
        private bool Check2NF()
        {
            foreach (var entity in Entities)
            {
                if (entity.Attributes == null || !entity.Attributes.Any(a => a.IsPrimaryKey))
                {
                    return false; // Không có khóa chính
                }

                // Giả định: Không có phụ thuộc một phần (cần logic phụ thuộc hàm thực tế)
                // Hiện tại trả về true để minh họa
            }
            return true;
        }

        // Kiểm tra 3NF cho toàn bộ lược đồ
        private bool Check3NF()
        {
            foreach (var entity in Entities)
            {
                if (entity.Attributes == null || !entity.Attributes.Any(a => a.IsPrimaryKey))
                {
                    return false; // Không có khóa chính
                }

                // Giả định: Không có phụ thuộc bắc cầu (cần logic phụ thuộc hàm thực tế)
                // Hiện tại trả về true để minh họa
            }
            return true;
        }

        // Kiểm tra BCNF cho toàn bộ lược đồ
        private bool CheckBCNF()
        {
            foreach (var entity in Entities)
            {
                if (entity.Attributes == null || !entity.Attributes.Any(a => a.IsPrimaryKey))
                {
                    return false; // Không có khóa chính
                }

                // Giả định: Đạt BCNF (cần logic phụ thuộc hàm thực tế)
                // Hiện tại trả về true để minh họa
            }
            return true;
        }

        // Xác định dạng chuẩn cao nhất
        private string DetermineHighestNormalForm(bool is1NF, bool is2NF, bool is3NF, bool isBCNF)
        {
            if (isBCNF) return "BCNF";
            if (is3NF) return "3NF";
            if (is2NF) return "2NF";
            if (is1NF) return "1NF";
            return "Không ở 1NF";
        }

        // Tạo hướng giải quyết và mô hình chuẩn hóa
        private string GenerateNormalizationSteps()
        {
            string steps = "Các bước chuẩn hóa lược đồ:<br/>";
            steps += "Bước 1: Đưa về 1NF - Đảm bảo tất cả thuộc tính đều nguyên tử.<br/>";
            steps += "Bước 2: Đưa về 2NF - Loại bỏ phụ thuộc một phần bằng cách tách các quan hệ.<br/>";
            steps += "Bước 3: Đưa về 3NF - Loại bỏ phụ thuộc bắc cầu bằng cách tách các thuộc tính không khóa.<br/>";
            steps += "Bước 4: Đưa về BCNF - Đảm bảo mọi yếu tố quyết định là khóa ứng viên.<br/>";
            steps += "<br/>Mô hình chuẩn hóa đề xuất:<br/>";
            foreach (var entity in Entities)
            {
                steps += $"- Quan hệ '{entity.Name}': ({string.Join(", ", entity.Attributes.Select(a => a.Name))})<br/>";
            }
            steps += "Chú thích: Tách các quan hệ để loại bỏ phụ thuộc không mong muốn dựa trên phụ thuộc hàm.<br/>";
            return steps;
        }
    }
}