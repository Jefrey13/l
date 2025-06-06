using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.RequestDtos.ConversationDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface IConversationService
    {
        Task<IEnumerable<ConversationResponseDto>> GetAllAsync(CancellationToken cancellation = default);

       Task<ConversationResponseDto> StartAsync(StartConversationRequest request, CancellationToken cancellation = default);

        Task<IEnumerable<ConversationResponseDto>> GetPendingAsync(CancellationToken cancellation = default);

        Task AssignAgentAsync(int conversationId, int agentUserId, string status, string jwtToken, CancellationToken cancellation = default);

        Task<ConversationResponseDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task CloseAsync(int conversationId, CancellationToken cancellation = default);

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

        Task<ConversationResponseDto> UpdateConversationAssignmentStateAsync(UpdateConversationRequestDto updateConversationRequestDto, CancellationToken cancellation = default);
    }
}