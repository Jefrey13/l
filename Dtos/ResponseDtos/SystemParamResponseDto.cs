using CustomerService.API.Utils.Enums;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class SystemParamResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}