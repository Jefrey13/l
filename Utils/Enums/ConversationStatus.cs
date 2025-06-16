namespace CustomerService.API.Utils.Enums
{
    /// <summary>
    /// Estados posibles de una conversación.
    /// </summary>
    public enum ConversationStatus
    {
        New,
        Bot,
        Waiting,
        Human,
        Closed,
        Incomplete,
    }
}