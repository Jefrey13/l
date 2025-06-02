namespace CustomerService.API.Dtos.ResponseDtos
{
    public class RoleResponseDto
    {
        public int RoleId { get; init; }
        public string RoleName { get; init; } = "";
        public string? Description { get; init; }
    }
}