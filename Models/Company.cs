using System;
using System.Collections.Generic;

namespace CustomerService.API.Models;

public partial class Company
{
    public int CompanyId { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    //public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<ContactLog> ContactLogs { get; set; }
        = new List<ContactLog>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
