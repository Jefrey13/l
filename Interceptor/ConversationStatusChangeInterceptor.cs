using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Interceptor
{
    public class ConversationStatusChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConversationStatusChangeInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            try
            {

                var context = eventData.Context;
                if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

                var entries = context.ChangeTracker
                    .Entries<Conversation>()
                    .Where(e => e.State == EntityState.Modified);

                foreach (var entry in entries)
                {
                    var oldStatus = (ConversationStatus?)entry.OriginalValues[nameof(Conversation.Status)];
                    var newStatus = (ConversationStatus?)entry.CurrentValues[nameof(Conversation.Status)];

                    if (oldStatus != newStatus && newStatus != null)
                    {
                        var httpContext = _httpContextAccessor.HttpContext;
                        var userIdStr = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        int.TryParse(userIdStr, out int userId);

                        var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
                        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

                        context.Add(new ConversationHistoryLog
                        {
                            ConversationId = (int)entry.CurrentValues[nameof(Conversation.ConversationId)],
                            OldStatus = oldStatus!.Value,
                            NewStatus = newStatus!.Value,
                            ChangedByUserId = userId,
                            ChangedAt = DateTime.UtcNow,
                            SourceIp = ip,
                            UserAgent = userAgent
                        });
                    }
                }

                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return await base.SavingChangesAsync(eventData, result, cancellationToken);
            }
        }
    }
}