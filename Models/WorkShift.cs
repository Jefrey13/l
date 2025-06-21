using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerService.API.Models
{
    public class WorkShift_User
    {
        public int Id { get; set; }
        public int OpeningHourId { get; set; }
        public int AssingedUserId { get; set; }
        public int  CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual OpeningHour OpeningHour { get; set; }
        public virtual User AssignedUser { get; set; }
        public virtual User? CreatedBy { get; set; }
        public virtual User? UpdatedBy { get; set; }
        public byte[] RowVersion { get; set; } = null!;
    }
}