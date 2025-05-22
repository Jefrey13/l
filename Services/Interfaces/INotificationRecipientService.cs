using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface INotificationRecipientService
    {
        /// <summary>
        /// Devuelve todas las notificaciones (leídas y no leídas) paginadas para un usuario.
        /// </summary>
        Task<PagedResponse<NotificationDto>> GetByUserAsync(
            PaginationParams @params,
            int userId,
            CancellationToken cancellation = default);

        /// <summary>
        /// Devuelve solo las notificaciones no leídas paginadas para un usuario.
        /// </summary>
        Task<PagedResponse<NotificationDto>> GetUnreadByUserAsync(
            PaginationParams @params,
            int userId,
            CancellationToken cancellation = default);

        /// <summary>
        /// Devuelve el conteo de notificaciones no leídas de un usuario.
        /// </summary>
        Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellation = default);

        /// <summary>
        /// Marca una notificación concreta como leída.
        /// </summary>
        Task MarkAsReadAsync(
            int notificationRecipientId,
            CancellationToken cancellation = default);

        /// <summary>
        /// Marca todas las notificaciones de un usuario como leídas.
        /// </summary>
        Task MarkAllReadAsync(
            int userId,
            CancellationToken cancellation = default);
    }
}