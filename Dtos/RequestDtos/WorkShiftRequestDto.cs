using CustomerService.API.Models;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class WorkShiftRequestDto
    {
        public int OpeningHourId { get; set; }
        public int AssignedUserId { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
    }
}