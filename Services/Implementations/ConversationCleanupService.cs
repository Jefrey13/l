using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CustomerService.API.Services.Implementations
{
    public class ConversationCleanupService : BackgroundService
    {
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly INotificationService _notification;

        public ConversationCleanupService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification)
        {
            _uow = uow;
            _nicDatetime = nicDatetime;
            _notification = notification;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldConversationsAsync(stoppingToken);
                }
                catch
                {
                    // log error
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CleanupOldConversationsAsync(CancellationToken ct)
        {
            var now = await _nicDatetime.GetNicDatetime();
            var cutoff = now.AddHours(-24);

            var oldConvs = await _uow.Conversations.GetAll()
                .Where(c => c.Status != ConversationStatus.Closed
                         && c.CreatedAt <= cutoff)
                .ToListAsync(ct);

            if (!oldConvs.Any())
                return;

            foreach (var conv in oldConvs)
            {
                conv.Status = ConversationStatus.Closed;
                conv.ClosedAt = now;
                _uow.Conversations.Update(conv);

                // notify the assigned agent if any, otherwise all admins
                var recipients = conv.AssignedAgentId.HasValue
                    ? new[] { conv.AssignedAgentId.Value }
                    : await _uow.Users.GetAll()
                        .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
                        .Select(u => u.UserId)
                        .ToArrayAsync(ct);

                var payload = new { conv.ConversationId, closedAt = now };
                await _notification.CreateAsync(
                    NotificationType.ConversationClosed,
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    recipients,
                    ct);
            }

            await _uow.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(
        UpdateConversationRequest request,
        CancellationToken cancellation = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var conv = await _uow.Conversations
                .GetByIdAsync(request.ConversationId, cancellation)
                ?? throw new KeyNotFoundException($"Conversation {request.ConversationId} not found.");


            if (request.Priority.HasValue)
                conv.Priority = request.Priority.Value;
            if (request.Status.HasValue)
                conv.Status = request.Status.Value;
            if (request.AssignedAgentId.HasValue)
                conv.AssignedAgentId = request.AssignedAgentId;
            if (request.IsArchived.HasValue)
                conv.IsArchived = request.IsArchived.Value;
            conv.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.Conversations.Update(conv);
            await _uow.SaveChangesAsync(cancellation);
        }
    }
}