using System;

namespace CustomerService.API.Dtos.ResponseDtos
{
    public class PresenceResponseDto
    {
        public DateTime? LastOnline { get; set; }
        public bool IsOnline { get; set; }
    }
}