using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class StatusType
    {
        public StatusType()
        {
            Tickets = new HashSet<Ticket>();
        }

        public string StatusTypeId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
