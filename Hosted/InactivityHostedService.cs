using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Data.Context;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils.Enums;
using CustomerService.API.WhContext;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerService.API.Hosted
{
    public class InactivityHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly MessagePrompts _prompts;
        private readonly ILogger<InactivityHostedService> _logger;

        // Umbrales fijos
        private readonly TimeSpan _warningThreshold = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _closeThreshold = TimeSpan.FromMinutes(2);

        public InactivityHostedService(
            IServiceScopeFactory scopeFactory,
            IHubContext<ChatHub> hubContext,
            IOptions<MessagePrompts> promptOpts,
            ILogger<InactivityHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _prompts = promptOpts.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Guardamos el TimeZoneInfo una sola vez
            TimeZoneInfo nicaraguaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Managua");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1) Esperar 1 minuto antes de cada ciclo
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                    // 2) Recalcular “ahoraNic” aquí, en cada iteración, para que sea la fecha/hora real
                    DateTime ahoraUtc = DateTime.UtcNow;
                    DateTime ahoraNic = TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, nicaraguaTimeZone);
                    // Eliminar milisegundos (opcional, para comparar segundo a segundo)
                    ahoraNic = new DateTime(
                        ahoraNic.Year,
                        ahoraNic.Month,
                        ahoraNic.Day,
                        ahoraNic.Hour,
                        ahoraNic.Minute,
                        ahoraNic.Second,
                        0
                    );

                    // 3) Crear un scope para resolver servicios scoped
                    using var scope = _scopeFactory.CreateScope();

                    // 4) Obtener el DbContext en modo tracking (para que EF detecte cambios automáticamente)
                    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerSupportContext>();

                    // 5) Obtener el service para enviar mensajes
                    var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                    // 6) Cargar las conversaciones pendientes (estado Bot, no cerradas y no archivadas),
                    //    **sin** AsNoTracking para que EF las rastree.
                    var pendientes = await dbContext.Conversations
                        .Where(c =>
                            c.Status == ConversationStatus.Bot)
                        .Include(c => c.Messages)  // si necesitas historial; si no, puedes omitirlo
                        .ToListAsync(stoppingToken);

                    // 7) Iterar sobre cada conversación pendiente
                    foreach (var conv in pendientes)
                    {
                        // A) Última vez que habló el cliente (o bien, fecha de creación si Cliente nunca habló)
                        DateTime lastClientDt = conv.ClientLastMessageAt ?? conv.CreatedAt;

                        // B) Calculamos la diferencia entre la hora de Nicaragua y el último mensaje del cliente
                        var diff = ahoraNic - lastClientDt;

                        // === 1) Enviar advertencia a los 2 minutos si aún no se envió ===
                        if (diff >= _warningThreshold && conv.WarningSentAt == null)
                        {
                            string warningText = _prompts.InactivityWarning;

                            // Llamada posicional: (SendMessageRequest, bool isFromBot, CancellationToken)
                            await messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = conv.ConversationId,
                                SenderId = 1, // BotUserId (se infiere de tu configuración)
                                Content = warningText,
                                MessageType = MessageType.Text
                            },
                            /* isFromBot: */ false,
                            /* cancellationToken: */ stoppingToken);

                            // Marcar en la entidad rastreada el campo WarningSentAt
                            conv.WarningSentAt = ahoraNic;

                            // **IMPORTANTE**: al estar en modo tracking, basta con SaveChangesAsync
                            await dbContext.SaveChangesAsync(stoppingToken);

                            // Mapear a DTO para notificar por SignalR
                            var convDto = conv.Adapt<ConversationDto>();

                            // Notificar a administradores que hubo un cambio
                            await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", convDto, stoppingToken);

                            // Pasamos a la siguiente conversación sin intentar cerrar esta en el mismo ciclo
                            continue;
                        }

                        // === 2) Cerrar la conversación a los 4 minutos (si ya se envió la advertencia) ===
                        if (diff >= _closeThreshold && conv.WarningSentAt != null)
                        {
                            // Cambiar estado a Closed y registrar fecha de cierre
                            conv.Status = ConversationStatus.Closed;
                            conv.ClosedAt = ahoraNic;

                            // Guardar cambios de estado y ClosedAt
                            await dbContext.SaveChangesAsync(stoppingToken);

                            string closeText = _prompts.InactivityClosed;
                            await messageService.SendMessageAsync(new SendMessageRequest
                            {
                                ConversationId = conv.ConversationId,
                                SenderId = 1,
                                Content = closeText,
                                MessageType = MessageType.Text
                            },
                            /* isFromBot: */ false,
                            /* cancellationToken: */ stoppingToken);

                            var convDto = conv.Adapt<ConversationDto>();

                            await _hubContext.Clients
                                .Group("Admin")
                                .SendAsync("ConversationUpdated", convDto, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // El servicio recibió solicitud de cancelación → salir del bucle
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en InactivityHostedService durante la ejecución periódica.");
                }
            }
        }
    }
}