using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserPreferencesService _userPreferencesService;

        public NotificationService(IMapper mapper, 
            IUserPreferencesService userPreferencesService,
            INotificationRepository notificationRepository, 
            ITicketRepository ticketRepository)
        {
            _notificationRepository = notificationRepository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _userPreferencesService = userPreferencesService;
        }

        /// <summary>
        /// Helper method to create a notification.
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="updateType"></param>
        /// <param name="isReassigned"></param>
        /// <param name="agentId"></param>
        public void CreateNotification(Ticket ticket, int? updateType, bool? isReassigned, string agentId = null)
        {
            var userId = ticket.UserId;
            var ticketId = ticket.TicketId;
            var updateTypeString = updateType.ToString();
            var (title, type, message) = updateType switch
            {
                1 => ("New Ticket Created Successfully", updateTypeString, $"Ticket #{ticketId} Successfully created."), // ticket created
                2 => ("Ticket Priority Updated", updateTypeString, $"Ticket #{ticketId} Priority has been updated."), // ticket priority update
                3 => ("Ticket Status Updated", updateTypeString, $"Ticket #{ticketId} Status has been updated."), // ticket status update
                4 => ("Ticket Details Updated", updateTypeString, $"Ticket #{ticketId} Details have been updated."), // ticket details update
                5 => ("Ticket Assignment Updated", updateTypeString, $"Ticket #{ticketId} Agent assignment updated."), // ticket assignment update
                6 => ("Ticket Has a New Comment", updateTypeString, $"Ticket #{ticketId} received a new comment."), // ticket new comment
                7 => ("Ticket Description Updated", updateTypeString, $"Ticket #{ticketId} Description has been updated."), // ticket new feedback
                8 => (string.Empty, string.Empty, string.Empty), // ticket reminder
                9 => (string.Empty, string.Empty, string.Empty), // team assignment update
                _ => (string.Empty, string.Empty, string.Empty)
            };

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(type))
            {
                if (agentId != null)
                {
                    AddNotification(ticketId, title, type, agentId, message);
                }
                AddNotification(ticketId, title, type, userId, message);
            }
            else if (isReassigned.HasValue)
            {
                string agentNotificationTitle = "New Ticket Assignment";
                string userNotificationTitle = isReassigned.Value ? "Ticket Reassigned to Another Agent" : "Ticket Assigned to an Agent";

                AddNotification(ticketId, agentNotificationTitle, "5", agentId, $"Ticket #{ticketId} has been assigned to you.");
                AddNotification(ticketId, userNotificationTitle, "5", userId, $"Ticket #{ticketId} has been assigned to an agent.");
            }
        }

        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Marks the notification as read.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        public void MarkNotificationAsRead(string notificationId)
        {
            var notification = _notificationRepository.FindById(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
            }
        }
        /// <summary>
        /// Marks the notification as unread.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        public void MarkNotificationAsUnread(string notificationId)
        {
            var notification = _notificationRepository.FindById(notificationId);
            if (notification != null)
            {
                notification.IsRead = false;
                _notificationRepository.Update(notification);
            }
        }
        /// <summary>
        /// Marks all notifications as unread.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void MarkAllNotificationsAsUnread(string userId)
        {
            var notifications = _notificationRepository.RetrieveAll().Where(n => n.UserId == userId).ToList();
            foreach (var notification in notifications)
            {
                notification.IsRead = false;
                _notificationRepository.Update(notification);
            }
        }
        /// <summary>
        /// Marks all notifications as read.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void MarkAllNotificationsAsRead(string userId)
        {
            var notifications = _notificationRepository.RetrieveAll().Where(n => n.UserId == userId).ToList();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
            }
        }
        /// <summary>
        /// Gets the unread notifications.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void GetUnreadNotifications(string UserId)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Adds the notification.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="notificationTypeId">The notification type identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="title">The title.</param>
        public void AddNotification(string ticketId, string description, string notificationTypeId, string userId, string title, string teamId = null)
        {
            bool sendNotification = WillUserBeNotified(userId, notificationTypeId);
            if (!sendNotification) return;

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid().ToString(),
                NotificationDate = DateTime.Now,
                TicketId = ticketId,
                TeamId = teamId,
                Description = description,
                Title = title,
                UserId = userId,
                NotificationTypeId = notificationTypeId
            };
            _notificationRepository.Add(notification);
        }

        private bool WillUserBeNotified(string userId, string notificationTypeId)
        {
            bool sendNotification = true;
            string condition = "no";
            sendNotification = notificationTypeId switch
            {
                "1" => GetNotificationPreference(userId, "notifyCreation") != condition,
                "2" => GetNotificationPreference(userId, "notifyStatusPriorityUpdate") != condition,
                "3" => GetNotificationPreference(userId, "notifyStatusPriorityUpdate") != condition,
                "4" => GetNotificationPreference(userId, "notifyDetailsUpdate") != condition,
                "5" => GetNotificationPreference(userId, "notifyAssignmentUpdate") != condition,
                "6" => GetNotificationPreference(userId, "notifyNewComment") != condition,
                "7" => GetNotificationPreference(userId, "notifyNewFeedback") != condition,
                "8" => GetNotificationPreference(userId, "notifyAgentReminder") != condition,
                "9" => GetNotificationPreference(userId, "notifyAgentTeamChange") != condition,
                _ => true
            };
            return sendNotification;
        }

        private string GetNotificationPreference(string userId, string key)
        {
            var preference = _userPreferencesService.GetUserPreferenceByKey(userId, key);
            if (preference == null) return "yes";
            return preference.Result.Value;
        }

        /// <summary>
        /// Determines whether [has unread notifications] [the specified user identifier].
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///   <c>true</c> if [has unread notifications] [the specified user identifier]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasUnreadNotifications(string userId)
        {
            return _notificationRepository.RetrieveAll().Any(n => n.UserId == userId && !n.IsRead);
        }
        /// <summary>
        /// Deletes the notification.
        /// </summary>
        /// <param name="notificationId">The notification identifier.</param>
        public void DeleteNotification(string notificationId)
        {
            _notificationRepository.Delete(notificationId);
        }

        public int GetUnreadNotificationCount(string userId)
        {
            return _notificationRepository.RetrieveAll().Count(n => n.UserId == userId && !n.IsRead);
        }

    }
}
