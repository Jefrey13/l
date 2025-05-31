using System;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class PresenceDto
    {
        public DateTime? LastOnline { get; set; }
        public bool IsOnline { get; set; }
    }
}