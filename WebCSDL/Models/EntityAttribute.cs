namespace WebCSDL.Models
{
    public class EntityAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsPrimaryKey { get; set; }
        public int EntityId { get; set; }
        public string? DataType { get; set; } // Cho phép null nếu kiểu dữ liệu chưa được chỉ định
        public Entity Entity { get; set; } = null!; // Quan hệ ngược lại
    }
}