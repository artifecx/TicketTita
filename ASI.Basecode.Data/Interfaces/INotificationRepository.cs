using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface INotificationRepository
    {
        IQueryable<Notification> RetrieveAll();
        void Add(Notification model);
        void Update(Notification model);
        void Delete(string notificationId);
        IQueryable<NotificationType> GetNotificationTypes();
        IQueryable<Ticket> GetTickets();

        Notification FindById(string notificationId);
        void AssignNotificationProperties(Notification notification);
        void SetNavigation(Notification notification);
    }
}
