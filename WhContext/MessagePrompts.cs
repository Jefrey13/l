namespace CustomerService.API.WhContext
{
    public class MessagePrompts
    {
        public string WelcomeBot { get; set; } = "";
        public string AskFullName { get; set; } = "";
        public string AskIdCard { get; set; } = "";
        public string InvalidIdFormat { get; set; } = "";
        public string DataComplete { get; set; } = "";
        public string InactivityWarning { get; set; } = "";
        public string InactivityClosed { get; set; } = "";
        public string SupportRequestReceived { get; set; } = "";
    }
}
