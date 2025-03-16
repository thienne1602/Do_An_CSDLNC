namespace WebCSDL.Models
{
    public class Relationship
    {
        public string Entity1 { get; set; } = null!;
        public string Entity2 { get; set; } = null!;
        public string Type { get; set; } = null!; // Ví dụ: "1-1", "1-N", "N-M"
    }
}