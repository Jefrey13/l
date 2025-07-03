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
using Humanizer;
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

        //// Umbrales fijos
        //private readonly TimeSpan _warningThreshold = TimeSpan.FromMinutes(1);
        //private readonly TimeSpan _closeThreshold = TimeSpan.FromMinutes(2);

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
                    var sysmParamService = scope.ServiceProvider.GetRequiredService<ISystemParamService>();
                    
                    var systemParams = await sysmParamService.GetAllAsync();

                    var _warningThreshold = systemParams
                        .FirstOrDefault(p => p.Name == "InactivityWarningThresholdTime");


                    // 6) Cargar las conversaciones pendientes (estado Bot, no cerradas y no archivadas),
                    //    **sin** AsNoTracking para que EF las rastree.
                    var pendientes = await dbContext.Conversations
                        .Where(c =>
                            c.Status == ConversationStatus.Bot || c.Status == ConversationStatus.Waiting || c.Status == ConversationStatus.Human)
                        .Include(c => c.Messages)  // si necesitas historial; si no, puedes omitirlo
                        .ToListAsync(stoppingToken);

                    var senderId = 1;
                    // 7) Iterar sobre cada conversación pendiente
                    foreach (var conv in pendientes)
                    {
                        if (conv.Status == ConversationStatus.Bot)
                        {
                            // A) Última vez que habló el cliente (o bien, fecha de creación si Cliente nunca habló)
                            DateTime lastClientDt = conv.ClientLastMessageAt ?? conv.CreatedAt;

                            int diff = 0;
                            if (lastClientDt.Minute > ahoraNic.Minute) diff = lastClientDt.Minute - ahoraNic.Minute;
                            else diff = ahoraNic.Minute - lastClientDt.Minute;

                            // B) Calculamos la diferencia entre la hora de Nicaragua y el último mensaje del cliente
                            int minuteParams = int.Parse(_warningThreshold.Value);

                            // === 1) Enviar advertencia a los 2 minutos si aún no se envió ===
                            if (diff >= minuteParams && conv.WarningSentAt == null)
                            {
                                //string warningText = _prompts.InactivityWarning;
                                string warningText = systemParams
                                    .FirstOrDefault(p => p.Name == "InactivityWarningThresholdMesssage")?.Value ??
                                    "Advertencia: No hemos recibido mensajes recientes. Responderemos pronto.";

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
                                var convDto = conv.Adapt<ConversationResponseDto>();

                                // Notificar a administradores que hubo un cambio
                                await _hubContext.Clients
                                    .Group("Admin")
                                    .SendAsync("ConversationUpdated", convDto, stoppingToken);

                                // Pasamos a la siguiente conversación sin intentar cerrar esta en el mismo ciclo
                                continue;
                            }

                            //var _closeThreshold = systemParams
                            //    .FirstOrDefault(p => p.Name == "InactivityCloseThreshold")?.Value is string closeThresholdStr &&
                            //    TimeSpan.TryParse(closeThresholdStr, out var parsedCloseThreshold)
                            //    ? parsedCloseThreshold
                            //    : TimeSpan.FromMinutes(4); // Valor por defecto si no se encuentra o no es válido

                            var _closeThreshold = systemParams
                                .FirstOrDefault(p => p.Name == "WaitWarningCloseTime");

                            var closedMinutes = int.Parse(_closeThreshold.Value);
                            // === 2) Cerrar la conversación a los 4 minutos (si ya se envió la advertencia) ===
                            //if (diff >= closedMinutes && conv.WarningSentAt != null)
                            //Quitamos la validación de WarningSentAt, para que la envie mas de 1 ves cada 2 minutos.
                            //A Como estaba solo se envia 1 ves la advertencia y luego no la enviaba mas.

                            if (diff >= closedMinutes)
                                {
                                try
                                {
                                    // Cambiar estado a Closed y registrar fecha de cierre
                                    conv.Status = ConversationStatus.Incomplete;
                                    conv.IncompletedAt = ahoraNic;

                                    // Guardar cambios de estado y ClosedAt
                                    await dbContext.SaveChangesAsync(stoppingToken);

                                    //string closeText = _prompts.InactivityClosed;
                                    string closeText = systemParams
                                        .FirstOrDefault(p => p.Name == "WaitWarningCloseMesssage")?.Value ??
                                        "La conversación se ha cerrado por inactividad. Si necesitas ayuda, por favor inicia una nueva conversación.";

                                    await messageService.SendMessageAsync(new SendMessageRequest
                                    {
                                        ConversationId = conv.ConversationId,
                                        SenderId = 1,
                                        Content = closeText,
                                        MessageType = MessageType.Text
                                    },
                                    /* isFromBot: */ false,
                                    /* cancellationToken: */ stoppingToken);

                                    var convDto = conv.Adapt<ConversationResponseDto>();

                                    await _hubContext.Clients
                                        .Group("Admin")
                                        .SendAsync("ConversationUpdated", convDto, stoppingToken);

                                } catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }

                        if (conv.Status == ConversationStatus.Waiting)
                        {

                            var RequestUnderReviewTime = await sysmParamService.GetByNameAsync("RequestUnderReviewTime");

                            if (((ahoraNic - conv.RequestedAgentAt.Value).TotalMinutes > int.Parse(RequestUnderReviewTime.Value)))
                            {
                                var RequestUnderReviewMessage = await sysmParamService.GetByNameAsync("RequestUnderReviewMessage");

                                await messageService.SendMessageAsync(new SendMessageRequest
                                {
                                    ConversationId = conv.ConversationId,
                                    SenderId = 1, // BotUserId (se infiere de tu configuración)
                                    Content = RequestUnderReviewMessage?.Value ?? "⏳ Su solicitud está en proceso. Un agente le atenderá en breve, por favor espere un momento.",

                                    MessageType = MessageType.Text
                                },
                            /* isFromBot: */ false,
                            /* cancellationToken: */ stoppingToken);


                                var convDto = conv.Adapt<ConversationResponseDto>();

                                await _hubContext.Clients
                                    .Group("Admin")
                                    .SendAsync("ConversationUpdated", convDto, stoppingToken);
                            }
                        } 
                        
                        if (conv.Status == ConversationStatus.Human || conv.Status == ConversationStatus.Waiting)
                        {
                            try
                            {
                                var UnclosedConversationTime = await sysmParamService.GetByNameAsync("UnclosedConversationTime");

                                //Marcar conversación con estado incompleto si se ha dejada colga por um largo periodo de tiempo por el admin o un miebro de soporte..
                                if (((ahoraNic - (conv.AgentLastMessageAt.HasValue ? conv.AgentLastMessageAt.Value : conv.AssignedAt.Value)).TotalMinutes > int.Parse(UnclosedConversationTime.Value) && conv.Status == ConversationStatus.Human)
                                    || ((ahoraNic - conv.AgentRequestAt.Value).TotalMinutes > int.Parse(UnclosedConversationTime.Value)) && conv.Status == ConversationStatus.Waiting)
                                {
                                    var UnclosedConversationMessage = await sysmParamService.GetByNameAsync("UnclosedConversationMessage");

                                    await messageService.SendMessageAsync(new SendMessageRequest
                                    {
                                        ConversationId = conv.ConversationId,
                                        SenderId = senderId,
                                        Content = UnclosedConversationMessage?.Value ?? "⏳ La conversación ha sido cerrada por falta de actividad. Ahora puede hablar con Milena de nuevo.",
                                        MessageType = MessageType.Text
                                    },
                                /* isFromBot: */ false,
                                /* cancellationToken: */ stoppingToken);

                                    conv.IncompletedAt = ahoraNic;
                                    conv.Status = ConversationStatus.Incomplete;
                                    await dbContext.SaveChangesAsync(stoppingToken);

                                    var convDto = conv.Adapt<ConversationResponseDto>();

                                    await _hubContext.Clients
                                        .Group("Admin")
                                        .SendAsync("ConversationUpdated", convDto, stoppingToken);
                                }
                            }catch(Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    
                    }

                    //Notificar al cliente que su solicitud esta ciendo procesada.
                    
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