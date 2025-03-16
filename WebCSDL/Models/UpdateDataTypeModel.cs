namespace WebCSDL.Models
{
    public class UpdateDataTypeModel
    {
        public string EntityName { get; set; } = null!; // Sửa thành required
        public string AttributeName { get; set; } = null!; // Sửa thành required
        public string DataType { get; set; } = null!; // Sửa thành required
    }
}