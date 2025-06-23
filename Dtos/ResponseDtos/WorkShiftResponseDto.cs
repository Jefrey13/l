using CustomerService.API.Models;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class WorkShiftResponseDto
    {
        public int Id { get; set; }
        public int OpeningHourId { get; set; }
        public OpeningHourResponseDto OpeningHour { get; set; } = null!;
        public int AssignedUserId { get; set; }
        public UserResponseDto AssignedUser { get; set; } = null!;
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public UserResponseDto CreatedBy { get; set; } = null!;
        public UserResponseDto? UpdatedBy { get; set; }
    }
}