using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public class WorkShift_User
    {
        public int OpeningHourId { get; set; }
        public int AssingedUserId { get; set; }
        public int  CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public OpeningHour OpeningHour { get; set; }
        public User AssignedUser { get; set; }
        public User? CreatedBy { get; set; }
        public User? UpdateBy { get; set; }
        public byte[] RowVersion { get; set; } = null!;
    }
}