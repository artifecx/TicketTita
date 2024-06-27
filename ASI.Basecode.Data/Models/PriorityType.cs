using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class PriorityType
    {
        public PriorityType()
        {
            Tickets = new HashSet<Ticket>();
        }

        public string PriorityTypeId { get; set; }
        public string PriorityName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
