using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.RequestDtos.ConversationDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IEnumerable<ConversationResponseDto>> GetAllAsync(CancellationToken cancellation = default);
        Task<PagedResponse<ConversationResponseDto>> GetByState(PaginationParams @params, string state, CancellationToken cancellation = default);
        Task<ConversationResponseDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default);

        Task<IEnumerable<ConversationResponseDto>> GetPendingAsync(CancellationToken cancellation = default);

        Task AssignAgentAsync(int conversationId, int agentUserId, string status, string jwtToken, CancellationToken cancellation = default);

        Task<ConversationResponseDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task CloseAsync(int conversationId, CancellationToken cancellation = default);

        Task<IEnumerable<WaitingClientResponseDto>> GetWaitingClient(FilterDashboard filters, CancellationToken ct = default);
        Task<ConversationResponseDto> GetOrCreateAsync(int clientContactId, CancellationToken cancellation = default);

        Task UpdateAsync(UpdateConversationRequest request, CancellationToken cancellation = default);

        Task<int> GetAssignedCountAsync(int agentUserId, CancellationToken cancellation = default);

        Task<IEnumerable<ConversationResponseDto>> GetConversationByRole(string jwtToken, CancellationToken cancellation = default);

        Task UpdateTags(int id, List<string> request, CancellationToken ct = default);

        Task<IEnumerable<ConversationHistoryDto>> GetHistoryByContactAsync(int contactId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene el resumen de todas las conversaciones de un cliente.
        /// </summary>
        Task<string> SummarizeAllByContactAsync(int contactId, CancellationToken ct = default);

        Task RespondAssignmentAsync(int conversationId, bool accepted, string? comment, CancellationToken ct);
        Task ForceAssignAsync(int conversationId, bool forced, string? assignmentComment, CancellationToken ct);

        Task AutoAssingAsync(int convId, int userId, CancellationToken tc = default);

        Task MarkConversationReadAsync(int conversationId, string jwtToken, CancellationToken ct = default);

        Task<int> GetToneAsync(int ConversationId, CancellationToken ct = default);

        Task<IEnumerable<ConversationStatusCountResponseDto>> GetConversationsCountByDateRange(DateTime from, DateTime to, CancellationToken ct = default);
        Task<IEnumerable<AverageAssignmentTimeResponseDto>> AverageAssignmentTimeAsync(DateTime from, DateTime to,CancellationToken ct = default);

        Task<IEnumerable<AdminAsigmentResponseTimeResponseDto>> AssigmentResponseTimeAsync(DateTime from, DateTime to, CancellationToken ct = default);

        Task<ResponseAgentAverageResponseDto> ResponseAgentAverageAsync(FilterDashboard filters, CancellationToken ct = default);
    }
}