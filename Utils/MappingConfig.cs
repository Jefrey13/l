using System;
using System.Collections.Generic;
using System.Linq;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Utils.Enums;
using Mapster;

namespace CustomerService.API.Utils
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Usuarios
            config.NewConfig<CreateUserRequest, User>();
            config.NewConfig<User, UserResponseDto>()
                .Map(d => d.UserId, s => s.UserId)
                .Map(d => d.FullName, s => s.FullName)
                .Map(d => d.Identifier, s => s.Identifier)
                .Map(d => d.Phone, s => s.Phone)
                .Map(d => d.IsActive, s => s.IsActive)
                .Map(d => d.ImageUrl, s => s.ImageUrl)
                .Map(d => d.UpdatedAt, s => s.UpdatedAt)
                .Map(d => d.IsOnline, s => s.IsOnline)
                .Map(d => d.ClientType, s => s.ClientType)
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.Email, s => s.Email);

            // Conversación: creación
            config.NewConfig<CreateConversationRequest, Conversation>()
                //.Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.Status, _ => ConversationStatus.New)
                .Map(d => d.CreatedAt, _ => DateTime.UtcNow)
                .Map(d => d.Tags, s => s.Tags);

            config.NewConfig<UpdateConversationRequest, Conversation>()
             .IgnoreNullValues(true)
             .Map(d => d.Priority, s => s.Priority)
             .Map(d => d.Status, s => s.Status)
             .Map(d => d.AssignedAgentId, s => s.AssignedAgentId)
             .Map(d => d.RequestedAgentAt, s => s.RequestedAgentAt)
             .Map(d => d.IsArchived, s => s.IsArchived)
             .Map(d => d.UpdatedAt, _ => DateTime.UtcNow)
             .Map(d => d.Tags, s => s.Tags);

            // Conversación: entidad a DTO
            config.NewConfig<Conversation, ConversationResponseDto>()
                .Map(d => d.ConversationId, s => s.ConversationId)
                //.Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.AssignedAgentId, s => s.AssignedAgentId)
                .Map(d => d.AssignedByUserId, s => s.AssignedByUserId)
                .Map(d => d.AssignedAgentName, s => s.AssignedAgent != null ? s.AssignedAgent.FullName : null)
                .Map(d => d.ClientContactName, s => s.ClientContact.FullName == null ? s.ClientContact.WaName : s.ClientContact.FullName)
                .Map(d => d.AssignedByUserName, s => s.AssignedByUser != null ? s.AssignedByUser.FullName : null)
                .Map(d => d.ContactNumber, s => s.ClientContact != null ? s.ClientContact.Phone : null)
                .Map(d => d.Status, s => s.Status.ToString())
                .Map(d => d.Initialized, s => s.Initialized)
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d=> d.Justification, s=> s.Justification)
                .Map(d => d.AssignedAt, s => s.AssignedAt)
                .Map(d => d.FirstResponseAt, s => s.FirstResponseAt)
                .Map(d => d.ClientFirstMessage, s => s.ClientFirstMessage)
                .Map(d => d.ClientLastMessageAt, s => s.ClientLastMessageAt)
                .Map(d => d.AgentFirstMessageAt, s => s.AgentFirstMessageAt)
                .Map(d => d.AgentLastMessageAt, s => s.AgentLastMessageAt)
                .Map(d => d.ClosedAt, s => s.ClosedAt)
                .Map(d => d.UpdatedAt, s => s.UpdatedAt)
                .Map(d => d.AssignmentResponseAt, s => s.AssignmentResponseAt)
                .Map(d => d.AssignmentComment, s => s.AssignmentComment)
                //.Map(d => d.ClosedAt, s => s.ClosedAt)
                .Map(d => d.IsArchived, s => s.IsArchived)
                .Map(d => d.TotalMessages, s => s.Messages.Count)
                .Map(d => d.LastActivity, s => s.Messages.Any() ? s.Messages.Max(m => m.SentAt.UtcDateTime) : s.CreatedAt)
                .Map(d => d.Duration, s => (s.ClosedAt ?? DateTime.UtcNow) - s.CreatedAt)
                .Map(d => d.TimeToFirstResponse, s => s.FirstResponseAt.HasValue ? s.FirstResponseAt.Value - s.CreatedAt : (TimeSpan?)null)
                .Map(d => d.IsClosed, s => s.Status == ConversationStatus.Closed)
                .Map(d => d.Messages, s => s.Messages.Adapt<List<MessageResponseDto>>())
                .Map(d => d.Tags, s => s.Tags);

            // Mensajes
            config.NewConfig<SendMessageRequest, Message>()
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.Content, s => s.Content)
                .Map(d => d.MessageType, s => s.MessageType)
                .Map(d => d.SenderUserId, s => s.SenderId)
                .Map(d => d.SentAt, _ => DateTimeOffset.UtcNow);

            config.NewConfig<Message, MessageResponseDto>()
                .Map(d => d.MessageId, s => s.MessageId)
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.SenderUserId, s => s.SenderUserId)
                .Map(d => d.SenderUserName, s => s.SenderUser != null ? s.SenderUser.FullName : null)
                .Map(d => d.SenderContactId, s => s.SenderContactId)
                .Map(d => d.SenderContactName, s => s.SenderContact  != null ? (s.SenderContact.FullName != null ? s.SenderContact.FullName : s.SenderContact.WaName) : null)
                .Map(d => d.IsIncoming, s => s.SenderContactId != null)
                .Map(d => d.Content, s => s.Content)
                .Map(d => d.ExternalId, s => s.ExternalId)
                .Map(d => d.MessageType, s => s.MessageType)
                .Map(d => d.Status, s => s.Status)
                .Map(d => d.SentAt, s => s.SentAt)
                .Map(d => d.DeliveredAt, s => s.DeliveredAt)
                .Map(d => d.ReadAt, s => s.ReadAt)
                .Map(d => d.Attachments, s => s.Attachments.Adapt<List<AttachmentDto>>());

            // Contactos
            config.NewConfig<CreateContactLogRequestDto, ContactLog>();
            config.NewConfig<UpdateContactLogRequestDto, ContactLog>();


            config.NewConfig<ContactLog, ContactLogResponseDto>()
                .Map(d => d.Id, s => s.Id)
                .Map(d => d.WaName, s => s.WaName)
                .Map(d => d.WaId, s => s.WaId)
                .Map(d => d.WaUserId, s => s.WaUserId)
                .Map(d => d.Phone, s => s.Phone)
                .Map(d => d.FullName, s => s.FullName)
                .Map(d => d.IdType, s => s.IdType)
                .Map(d => d.IdCard, s => s.IdCard)
                .Map(d => d.ResidenceCard, s => s.ResidenceCard)
                .Map(d => d.Password, s => s.Password)
                .Map(d=> d.CompanyName, s=> s.CompanyName)
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d=> d.IsVerified, s=> s.IsVerified)
                .Map(d => d.Status, s => s.Status) 
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d => d.UpdatedAt, s => s.UpdatedAt)
                .Map(d => d.Company, s => s.Company);

            // Empresa
            config.NewConfig<Company, CompanyResponseDto>();

            // Notificaciones
            config.NewConfig<NotificationRecipient, NotificationResponseDto>()
               .Map(d => d.NotificationRecipientId, s => s.NotificationRecipientId)
               .Map(d => d.NotificationId, s => s.NotificationId)
               .Map(d => d.Type, s => s.Notification.Type)
               .Map(d => d.Payload, s => s.Notification.Payload)
               .Map(d => d.CreatedAt, s => s.Notification.CreatedAt)
               .Map(d => d.IsRead, s => s.IsRead);

            config.NewConfig<StartConversationRequest, Conversation>()
                //.Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.Status, _ => ConversationStatus.Bot)
                .Map(d => d.CreatedAt, _ => DateTime.UtcNow)
                .IgnoreNullValues(true)
                .Map(d => d.Tags, s => s.Tags);

            config.NewConfig<Notification, NotificationResponseDto>()
                .Map(d => d.NotificationId, s => s.NotificationId)
                .Map(d => d.Type, s => s.Type)
                .Map(d => d.Payload, s => s.Payload)
                .Map(d => d.CreatedAt, s => s.CreatedAt);

            // Eventos de dominio
            config.NewConfig<NewContactCreatedDto, NewContactCreatedDto>();
            config.NewConfig<SupportRequestedDto, SupportRequestedDto>();
            config.NewConfig<ConversationAssignedDto, ConversationAssignedDto>();

            config.NewConfig<SystemParamRequestDto, SystemParam>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Value, src => src.Value)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Type, src =>  src.Type)
                .IgnoreNullValues(true);

            config.NewConfig<SystemParam, SystemParamResponseDto>()
               .Map(dest => dest.Id, src => src.Id)
               .Map(dest => dest.Name, src => src.Name)
               .Map(dest => dest.Value, src => src.Value)
               .Map(dest => dest.Description, src => src.Description)
               .Map(dest => dest.IsActive, src => src.IsActive)
               .Map(dest => dest.Type, src => src.Type.ToString())
               .Map(dest => dest.CreatedAt, src => src.CreatedAt)
               .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
               .IgnoreNullValues(true);

            config.NewConfig<OpeningHour, OpeningHourResponseDto>()
                .Map(dest => dest.Recurrence, src => src.Recurrence)
                .Map(dest => dest.DaysOfWeek, src => src.DaysOfWeek)
                .Map(dest => dest.HolidayDate, src => src.HolidayDate)
                .Map(dest => dest.SpecificDate, src => src.SpecificDate)
                .Map(des => des.IsWorkShift, src => src.IsWorkShift)
                .Map(dest => dest.StartTime, src => src.StartTime)
                .Map(dest => dest.EndTime, src => src.EndTime)
                .Map(dest => dest.EffectiveFrom, src => src.EffectiveFrom)
                .Map(dest => dest.EffectiveTo, src => src.EffectiveTo)
                .Map(dest => dest.IsActive, src => src.IsActive);

            config.NewConfig<OpeningHourRequestDto, OpeningHour>()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Recurrence, src => src.Recurrence)
                .Map(dest => dest.DaysOfWeek, src => src.DaysOfWeek)
               .Map(des => des.IsWorkShift, src => src.IsWorkShift)
                .Map(dest => dest.HolidayDate, src => src.HolidayDate)
                .Map(dest => dest.SpecificDate, src => src.SpecificDate)
                .Map(dest => dest.StartTime, src => src.StartTime)
                .Map(dest => dest.EndTime, src => src.EndTime)
                .Map(dest => dest.EffectiveFrom, src => src.EffectiveFrom)
                .Map(dest => dest.EffectiveTo, src => src.EffectiveTo)
                .Map(dest => dest.IsHolidayMoved, src => src.IsHolidayMoved)
                .Map(dest => dest.HolidayMovedFrom, src => src.HolidayMovedFrom)
                .Map(dest => dest.HolidayMoveTo, src => src.HolidayMoveTo)
                .Map(dest => dest.IsActive, src => src.IsActive);

            config.NewConfig<WorkShift_User, WorkShiftResponseDto>()
                .Map(dest => dest.OpeningHourId, src => src.OpeningHourId)
                .Map(dest => dest.OpeningHour, src => src.OpeningHour)
                .Map(dest => dest.AssignedUserId, src => src.AssignedUserId)
                .Map(dest => dest.AssignedUser, src => src.AssignedUser)
                .Map(dest => dest.ValidFrom, src => src.ValidFrom)
                .Map(dest => dest.ValidTo, src => src.ValidTo)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
                .Map(dest => dest.CreatedById, src => src.CreatedById)
                .Map(dest => dest.UpdatedById, src => src.UpdatedById)
                .Map(dest => dest.CreatedBy, src => src.CreatedBy)
                .Map(dest => dest.UpdatedBy, src => src.UpdatedBy);

            config.NewConfig<WorkShiftRequestDto, WorkShift_User>()
                .Map(dest => dest.OpeningHourId, src => src.OpeningHourId)
                .Map(dest => dest.AssignedUserId, src => src.AssignedUserId)
                .Map(dest => dest.ValidFrom, src => src.ValidFrom)
                .Map(dest => dest.ValidTo, src => src.ValidTo)
                .Map(dest => dest.IsActive, src => src.IsActive);

        }
    }
}