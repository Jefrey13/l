namespace CustomerService.API.Dtos.ResponseDtos
{
    public class OpeningHourResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Horarios específicos (solo hora, sin fecha completa)
        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public bool? IsHoliday { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int CreatedById { get; set; }

        public int UpdatedById { get; set; }

        public virtual UserResponseDto CreatedBy { get; set; } = null!;

        public virtual UserResponseDto UpdatedBy { get; set; } = null!;
    }
}
