using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Repositories.Interfaces;

namespace CustomerService.API.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IAuthTokenRepository AuthTokens { get; }
        IUserRoleRepository UserRoles { get; }
        ICompanyRepository Companies { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        IAttachmentRepository Attachments { get; }
        IMenuRepository Menus { get; }
        IRoleMenuRepository RoleMenus { get; }
        IContactLogRepository ContactLogs { get; }
        INotificationRepository Notifications { get; }
        INotificationRecipientRepository NotificationRecipients { get; }

        ISystemParamRepository SystemParamRepository { get; }

        IOpeningHourRepository OpeningHours { get; }

        IWorkShiftRepository WorkShifts { get; }
        void ClearChangeTracker();
        Task<int> SaveChangesAsync(CancellationToken cancellation = default);
    }
}