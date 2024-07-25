using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface INotificationService
    {
        void CreateTicketNotification(Ticket ticket, int? updateType, bool? isReassigned, string agentId = null);
        IEnumerable<NotificationViewModel> RetrieveAll(string userId);
        void MarkNotificationAsRead(string notificationId);
        void MarkAllNotificationsAsRead(string UserId);
        void AddNotification(string ticketId, string description, string notificationTypeId,string UserId, string title, string teamId = null);
        void GetUnreadNotifications(string userId);
        public void MarkAllNotificationsAsUnread(string UserId);
        void MarkNotificationAsUnread(string notificationId);
        bool HasUnreadNotifications(string userId);
        void DeleteNotification(string notificationId);
        int GetUnreadNotificationCount(string userId);
    }
}
