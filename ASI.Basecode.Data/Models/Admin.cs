using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Admin
    {
        public Admin()
        {
            TicketAssignments = new HashSet<TicketAssignment>();
    /*        UserCreatedByNavigations = new HashSet<User>();
            UserUpdatedByNavigations = new HashSet<User>();*/
        }

        public string AdminId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsSuper { get; set; }

        public virtual ICollection<TicketAssignment> TicketAssignments { get; set; }
/*        public virtual ICollection<User> UserCreatedByNavigations { get; set; }
        public virtual ICollection<User> UserUpdatedByNavigations { get; set; }*/
    }
}
