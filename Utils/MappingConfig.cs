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
            config.NewConfig<CreateUserRequest, User>();

            config.NewConfig<User, UserDto>();

            config.NewConfig<CreateConversationRequest, Conversation>()
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.Status, _ => ConversationStatus.New)
                .Map(d => d.CreatedAt, _ => DateTime.UtcNow);

            config.NewConfig<UpdateConversationRequest, Conversation>()
                .IgnoreNullValues(true)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.Status, s => s.Status)
                .Map(d => d.AssignedAgentId, s => s.AssignedAgentId)
                .Map(d => d.IsArchived, s => s.IsArchived)
                .Map(d => d.UpdatedAt, _ => DateTime.UtcNow);

            config.NewConfig<Conversation, ConversationDto>()
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.AssignedAgentId, s => s.AssignedAgentId)
                .Map(d => d.AssignedByUserId, s => s.AssignedByUserId)
                .Map(d => d.AssignedAgentName, s => s.AssignedAgent != null ? s.AssignedAgent.FullName : null)
                .Map(d => d.ClientContactName, s => s.ClientContact != null ? s.ClientContact.WaName : null)
                .Map(d => d.AssignedByUserName, s => s.AssignedByUser != null ? s.AssignedByUser.FullName : null)
                .Map(d => d.ContactNumber, s => s.ClientContact != null ? s.ClientContact.Phone : null)
                .Map(d => d.AssignedAt, s => s.AssignedAt)
                .Map(d => d.Status, s => s.Status)
                .Map(d => d.Initialized, s => s.Initialized)
                .Map(d => d.CreatedAt, s => s.CreatedAt)
                .Map(d => d.FirstResponseAt, s => s.FirstResponseAt)
                .Map(d => d.UpdatedAt, s => s.UpdatedAt)
                .Map(d => d.ClosedAt, s => s.ClosedAt)
                .Map(d => d.IsArchived, s => s.IsArchived)
                .Map(d => d.TotalMessages, s => s.Messages.Count)
                .Map(d => d.LastActivity, s => s.Messages.Any() ? s.Messages.Max(m => m.SentAt.UtcDateTime) : s.CreatedAt)
                .Map(d => d.Duration, s => (s.ClosedAt ?? DateTime.UtcNow) - s.CreatedAt)
                .Map(d => d.TimeToFirstResponse, s => s.FirstResponseAt.HasValue ? s.FirstResponseAt.Value - s.CreatedAt : (TimeSpan?)null)
                .Map(d => d.IsClosed, s => s.Status == ConversationStatus.Closed)
                .Map(d => d.Messages, s => s.Messages.Adapt<List<MessageDto>>())
                .Map(d => d.Tags, s => s.ConversationTags.Select(ct => ct.Tag).Adapt<List<TagDto>>());

            config.NewConfig<SendMessageRequest, Message>()
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.Content, s => s.Content)
                .Map(d => d.MessageType, s => s.MessageType)
                .Map(d => d.SenderUserId, s => s.SenderId)
                .Map(d => d.SentAt, _ => DateTimeOffset.UtcNow);

            config.NewConfig<Message, MessageDto>()
                .Map(d => d.MessageId, s => s.MessageId)
                .Map(d => d.ConversationId, s => s.ConversationId)
                .Map(d => d.SenderUserId, s => s.SenderUserId)
                .Map(d => d.SenderContactId, s => s.SenderContactId)
                .Map(d => d.IsIncoming, s => s.SenderContactId != null)
                .Map(d => d.Content, s => s.Content)
                .Map(d => d.ExternalId, s => s.ExternalId)
                .Map(d => d.MessageType, s => s.MessageType)
                .Map(d => d.Status, s => s.Status)
                .Map(d => d.SentAt, s => s.SentAt)
                .Map(d => d.DeliveredAt, s => s.DeliveredAt)
                .Map(d => d.ReadAt, s => s.ReadAt)
                .Map(d => d.Attachments, s => s.Attachments.Adapt<List<AttachmentDto>>());

            config.NewConfig<CreateContactLogRequestDto, ContactLog>();
            config.NewConfig<UpdateContactLogRequestDto, ContactLog>();
            config.NewConfig<ContactLog, ContactLogResponseDto>();

            config.NewConfig<Tag, TagDto>()
                .Map(d => d.TagId, s => s.TagId)
                .Map(d => d.Name, s => s.Name);

            config.NewConfig<Company, CompanyDto>();

            config.NewConfig<NotificationRecipient, NotificationDto>()
               .Map(d => d.NotificationRecipientId, s => s.NotificationRecipientId)
               .Map(d => d.NotificationId, s => s.NotificationId)
               .Map(d => d.Type, s => s.Notification.Type)
               .Map(d => d.Payload, s => s.Notification.Payload)
               .Map(d => d.CreatedAt, s => s.Notification.CreatedAt)
               .Map(d => d.IsRead, s => s.IsRead);

            config.NewConfig<StartConversationRequest, Conversation>()
                .Map(d => d.CompanyId, s => s.CompanyId)
                .Map(d => d.ClientContactId, s => s.ClientContactId)
                .Map(d => d.Priority, s => s.Priority)
                .Map(d => d.Status, _ => ConversationStatus.Bot)
                .Map(d => d.CreatedAt, _ => DateTime.UtcNow)
                .IgnoreNullValues(true);
            config.NewConfig<Notification, NotificationDto>()
                .Map(d => d.NotificationId, s => s.NotificationId)
                .Map(d => d.Type, s => s.Type)
                .Map(d => d.Payload, s => s.Payload)
                .Map(d => d.CreatedAt, s => s.CreatedAt);

            config.NewConfig<NewContactCreatedDto, NewContactCreatedDto>();
            config.NewConfig<SupportRequestedDto, SupportRequestedDto>();
            config.NewConfig<ConversationAssignedDto, ConversationAssignedDto>();
        }
    }
}