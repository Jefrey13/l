namespace CustomerService.API.Utils
{
    public class FilterDashboard
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? AgentId { get; set; }
        public List<int>? AgentsId { get; set; }
        public int? CompaniesId { get; set; }
        public int? AdminUserId { get; set; }
        public int? CustomerId { get; set; }
        public bool? Today { get; set; }
        public bool? Yesterday { get; set; }
        public bool? Tomorrow { get; set; }
    }
}