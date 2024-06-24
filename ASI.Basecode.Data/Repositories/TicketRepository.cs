using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace ASI.Basecode.Data.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        #region tables
        /// Added to simulate a database and relationships between tables
        private readonly List<Ticket> _ticketList = new List<Ticket>();
        private readonly List<CategoryType> _categoryTypes = new List<CategoryType>
        {
            new CategoryType { categoryType_ID = 1, categoryName = "Software", Description = "Software related issues" },
            new CategoryType { categoryType_ID = 2, categoryName = "Hardware", Description = "Hardware related issues" },
            new CategoryType { categoryType_ID = 3, categoryName = "Network", Description = "Network related issues" },
            new CategoryType { categoryType_ID = 4, categoryName = "Account", Description = "Account related issues" },
            new CategoryType { categoryType_ID = 5, categoryName = "Other", Description = "Other issues" }
        };
        private readonly List<PriorityType> _priorityTypes = new List<PriorityType>
        {
            new PriorityType { priorityType_ID = 1, priorityName = "Low", Description = "Low priority" },
            new PriorityType { priorityType_ID = 2, priorityName = "Medium", Description = "Medium priority" },
            new PriorityType { priorityType_ID = 3, priorityName = "High", Description = "High priority" },
            new PriorityType { priorityType_ID = 4, priorityName = "Severe", Description = "Severe priority" }
        };
        private readonly List<StatusType> _statusTypes = new List<StatusType>
        {
            new StatusType { statusType_ID = 1, statusName = "Open", Description = "Ticket is open" },
            new StatusType { statusType_ID = 2, statusName = "In Progress", Description = "Ticket is in progress" },
            new StatusType { statusType_ID = 3, statusName = "Resolved", Description = "Ticket is resolved" },
            new StatusType { statusType_ID = 4, statusName = "Closed", Description = "Ticket is closed" }
        };
        #endregion


        /// <summary>
        /// Gets all tickets.
        /// </summary>
        /// <returns>a list of all tickets (IEnumerable)</returns>
        public IEnumerable<Ticket> GetAll()
        {
            return _ticketList;
        }

        /// <summary>
        /// Adds a new ticket to the list.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Add(Ticket ticket)
        {
            ticket.statusType_ID = ticket.statusType_ID != 0 ? ticket.statusType_ID : 1;

            AssignTicketProperties(ticket);
            _ticketList.Add(ticket);
        }

        /// <summary>
        /// Updates an existing ticket in the list.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Update(Ticket ticket)
        {
            var existingTicket = FindById(ticket.ticket_ID);
            if (existingTicket != null)
            {
                existingTicket = ticket;
                AssignTicketProperties(existingTicket);
            }
        }

        /// <summary>
        /// Deletes the specified ticket found using the identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var existingTicket = FindById(id);
            if (existingTicket != null)
            {
                _ticketList.Remove(existingTicket);
            }
        }


        #region Helper Methods
        /// <summary>
        /// Finds a ticket by identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>a Ticket</returns>
        public Ticket FindById(string id)
        {
            return _ticketList.Where(x => x.ticket_ID.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the category by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a category type (CategoryType)</returns>
        public CategoryType FindCategoryById(int id)
        {
            return _categoryTypes.Where(x => x.categoryType_ID.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the priority by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a priority type (PriorityType)</returns>
        public PriorityType FindPriorityById(int id)
        {
            return _priorityTypes.Where(x => x.priorityType_ID.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the status by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a status type (StatusType)</returns>
        public StatusType FindStatusById(int id)
        {
            return _statusTypes.Where(x => x.statusType_ID.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Gets all category types.
        /// </summary>
        /// <returns>List of all category types (IEnumerable)</returns>
        public IEnumerable<CategoryType> GetCategoryTypes()
        {
            return _categoryTypes;
        }

        /// <summary>
        /// Gets all priority types.
        /// </summary>
        /// <returns>List of all priority types (IEnumerable)</returns>
        public IEnumerable<PriorityType> GetPriorityTypes()
        {
            return _priorityTypes;
        }

        /// <summary>
        /// Gets all status types.
        /// </summary>
        /// <returns>List of all status types (IEnumerable)</returns>
        public IEnumerable<StatusType> GetStatusTypes()
        {
            return _statusTypes;
        }

        /// <summary>
        /// Assigns the following ticket properties: ticket_ID, Category, Priority, Status, CategoryNumber
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// 
        /// Ticket ID format: CC-CN-NN
        /// CC = Category Type ID
        /// CN = Category Number (how many tickets have been created for a category)
        /// NN = Ticket Number (total number of tickets created)
        private void AssignTicketProperties(Ticket ticket)
        {
            /// Category Type ID (CC)
            int categoryTypeId = ticket.categoryType_ID;

            /// Category Number (CN)
            var lastTicketInCategory = _ticketList.LastOrDefault(t => t.categoryType_ID == ticket.categoryType_ID);
            int categoryTicketCount = lastTicketInCategory == null ? 1 : lastTicketInCategory.CategoryNumber + 1;

            /// Ticket Number (NN)
            int totalTicketCount = _ticketList.Count + 1;

            /// Set ticket id
            ticket.ticket_ID = $"{categoryTypeId:00}-{categoryTicketCount:00}-{totalTicketCount:00}";

            /// Update Category Number (CN) index
            lastTicketInCategory = _ticketList.LastOrDefault(t => t.categoryType_ID == ticket.categoryType_ID);
            ticket.CategoryNumber = lastTicketInCategory == null ? 1 : lastTicketInCategory.CategoryNumber + 1;

            /// Set Category, Priority, and Status
            ticket.Category = _categoryTypes.Single(x => x.categoryType_ID == ticket.categoryType_ID);
            ticket.Priority = _priorityTypes.Single(x => x.priorityType_ID == ticket.priorityType_ID);
            ticket.Status = _statusTypes.Single(x => x.statusType_ID == ticket.statusType_ID);
        }
        #endregion
    }
}
