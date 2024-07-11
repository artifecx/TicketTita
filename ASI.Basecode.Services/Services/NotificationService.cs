using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITicketRepository _ticketRepository;

        public NotificationService(IMapper mapper, INotificationRepository notificationRepository, ITicketRepository ticketRepository)
        {
            _notificationRepository = notificationRepository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        public IEnumerable<NotificationViewModel> RetrieveAll(string userId)
        {
            var notification = _notificationRepository.RetrieveAll().Where(x => x.UserId == userId).ToList();
            var data = notification.Select(s => new NotificationViewModel
            {
                NotificationId = s.NotificationId,
                NotificationDate = s.NotificationDate,
                NotificationTypeId = s.NotificationTypeId,
                TicketId = s.TicketId,
                Title = s.Title,
                UserId = s.UserId,
                Description = s.Description,
                IsRead = s.IsRead
            });

            return data;
        }

        public void MarkNotificationAsRead(string notificationId)
        {
            var notification = _notificationRepository.FindById(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
            }
        }


        public void MarkAllNotificationsAsRead(string ticketId)
        {
            throw new NotImplementedException();
        }

        public void GetUnreadNotifications(string UserId)
        {
            throw new NotImplementedException();
        }

        public void AddNotification(string ticketId, string description, string notificationTypeId, string userId, string title)
        {
            var ticket = _ticketRepository.FindById(ticketId);
            if (ticket != null)
            {
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    NotificationDate = DateTime.Now,
                    TicketId = ticketId,
                    Description = description,
                    Title = title,
                    UserId = userId,
                    NotificationTypeId = notificationTypeId
                };
                _notificationRepository.Add(notification);
            }
            else
            {
                throw new ArgumentException("Invalid ticket ID");
            }
        }
        public bool HasUnreadNotifications(string userId)
        {
            return _notificationRepository.RetrieveAll().Any(n => n.UserId == userId && !n.IsRead);
        }

        public void UpdateNotification(string notificationId)
        {
            throw new NotImplementedException();
        }

        public void DeleteNotification(string notificationId)
        {
            throw new NotImplementedException();
        }
    }
}
