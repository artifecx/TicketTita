using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface INotificationService
    {
        IEnumerable<NotificationViewModel> RetrieveAll(string userId);
        void MarkNotificationAsRead(string notificationId);
        void MarkAllNotificationsAsRead(string ticketId);
        void AddNotification(string ticketId, string description, string notificationTypeId,string UserId, string title);
        void GetUnreadNotifications(string userId);
        void UpdateNotification(string notificationId);
        void DeleteNotification(string notificationId);
    }
}
