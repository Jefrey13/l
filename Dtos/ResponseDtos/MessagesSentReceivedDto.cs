namespace CustomerService.API.Dtos.ResponseDtos
{
    public class MessagesSentReceivedDto
    {
        public IEnumerable<TemporalCountResponseDto> Sent { get; set; }
        public IEnumerable<TemporalCountResponseDto> Received { get; set; }
    }
}
