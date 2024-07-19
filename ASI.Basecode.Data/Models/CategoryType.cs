using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class CategoryType
    {
        public CategoryType()
        {
            Teams = new HashSet<Team>();
            Tickets = new HashSet<Ticket>();
        }

        public string CategoryTypeId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
