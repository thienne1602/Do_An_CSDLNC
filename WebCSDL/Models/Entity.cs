namespace WebCSDL.Models
{
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public byte[] RowVersion { get; set; } = null!; // Thêm nếu đã áp dụng concurrency
        public List<EntityAttribute> Attributes { get; set; } = new();
    }
}