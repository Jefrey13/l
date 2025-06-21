using CustomerService.API.Models;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class WorkShiftRequestDto
    {
        public int OpeningHourId { get; set; }
        public int AssingedUserId { get; set; }
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public bool IsActive { get; set; }
    }
}