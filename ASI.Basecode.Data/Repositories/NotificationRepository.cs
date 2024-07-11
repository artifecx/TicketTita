using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        private readonly List<NotificationType> _notificationTypes;
        private readonly List<Ticket> _tickets;

        public NotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _notificationTypes = GetNotificationTypes().ToList();
            _tickets = GetTickets().ToList();
        }

        public IQueryable<Notification> RetrieveAll()
        {
            var notifications = this.GetDbSet<Notification>();

            foreach (var notification in notifications)
            {
                notification.NotificationType = _notificationTypes.SingleOrDefault(nt => nt.NotificationTypeId == notification.NotificationTypeId);
                notification.Ticket = _tickets.SingleOrDefault(t => t.TicketId == notification.TicketId);
            }
            return notifications;
        }

        public void Add(Notification model)
        {
            AssignNotificationProperties(model);

            this.GetDbSet<Notification>().Add(model);
            UnitOfWork.SaveChanges();
        }

        public void Update(Notification model)
        {
            SetNavigation(model);
            this.GetDbSet<Notification>().Update(model);
            UnitOfWork.SaveChanges();
        }

        public void Delete(string notificationId)
        {
            var notificationToDelete = this.GetDbSet<Notification>().FirstOrDefault(n => n.NotificationId == notificationId);
            if (notificationToDelete != null)
            {
                this.GetDbSet<Notification>().Remove(notificationToDelete);
                UnitOfWork.SaveChanges();
            }
        }

        #region Helper Methods
        public IQueryable<NotificationType> GetNotificationTypes()
        {
            return this.GetDbSet<NotificationType>();
        }

        public IQueryable<Ticket> GetTickets()
        {
            return this.GetDbSet<Ticket>();
        }

        public Notification FindById(string notificationId)
        {
            var notification = this.GetDbSet<Notification>()
                                   .Include(n => n.NotificationType)
                                   .Include(n => n.Ticket)
                                   .FirstOrDefault(n => n.NotificationId == notificationId);
            return notification;
        }

        public void AssignNotificationProperties(Notification notification)
        {
            notification.NotificationType = _notificationTypes.SingleOrDefault(nt => nt.NotificationTypeId == notification.NotificationTypeId);
            notification.Ticket = _tickets.SingleOrDefault(t => t.TicketId == notification.TicketId);
        }

        public void SetNavigation(Notification notification)
        {
            notification.NotificationType = _notificationTypes.SingleOrDefault(nt => nt.NotificationTypeId == notification.NotificationTypeId);
            notification.Ticket = _tickets.SingleOrDefault(t => t.TicketId == notification.TicketId);
        }
        #endregion
    }
}
