using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Models
{
    public class SystemParam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public SystemParamType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public bool IsActive { get; set; } = true;

        public byte[] RowVersion { get; set; } = null!;
    }
}