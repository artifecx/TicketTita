using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class NotificationType
    {
        public NotificationType()
        {
            Notifications = new HashSet<Notification>();
        }

        public string NotificationTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
