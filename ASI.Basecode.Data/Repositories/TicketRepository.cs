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
    public class TicketRepository : BaseRepository, ITicketRepository
    {
        private readonly List<CategoryType> _categoryTypes;
        private readonly List<PriorityType> _priorityTypes;
        private readonly List<StatusType> _statusTypes;

        public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        { 
            _categoryTypes = GetCategoryTypes().ToList();
            _priorityTypes = GetPriorityTypes().ToList();
            _statusTypes = GetStatusTypes().ToList();
        }
        
        /// <summary>
        /// Gets all tickets.
        /// </summary>
        /// <returns>a list of all tickets (IEnumerable)</returns>
        public IQueryable<Ticket> GetAll()
        {
            var tickets = this.GetDbSet<Ticket>();

            foreach (Ticket t in tickets)
            {
                t.CategoryType = _categoryTypes.Single(x => x.CategoryTypeId == t.CategoryTypeId);
                t.PriorityType = _priorityTypes.Single(x => x.PriorityTypeId == t.PriorityTypeId);
                t.StatusType = _statusTypes.Single(x => x.StatusTypeId == t.StatusTypeId);
            }
            return tickets;
        }

        /// <summary>
        /// Adds a new ticket to the list.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Add(Ticket ticket)
        {
            ticket.StatusTypeId = ticket.StatusTypeId ?? "1";

            AssignTicketProperties(ticket);

            this.GetDbSet<Ticket>().Add(ticket);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Updates an existing ticket in the list.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Update(Ticket ticket)
        {
            // TODO: fix categoryType_ID is NULL cannot UPDATE
            this.GetDbSet<Ticket>().Update(ticket);
            UnitOfWork.SaveChanges();
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
                this.GetDbSet<Ticket>().Remove(existingTicket);
                UnitOfWork.SaveChanges();
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
            return this.GetDbSet<Ticket>().Where(x => x.TicketId.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the category by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a category type (CategoryType)</returns>
        public CategoryType FindCategoryById(string id)
        {
            return this.GetDbSet<CategoryType>().Where(x => x.CategoryTypeId.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the priority by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a priority type (PriorityType)</returns>
        public PriorityType FindPriorityById(string id)
        {
            return this.GetDbSet<PriorityType>().Where(x => x.PriorityTypeId.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Finds the status by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a status type (StatusType)</returns>
        public StatusType FindStatusById(string id)
        {
            return this.GetDbSet<StatusType>().Where(x => x.StatusTypeId.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Gets all category types.
        /// </summary>
        /// <returns>List of all category types (IEnumerable)</returns>
        public IQueryable<CategoryType> GetCategoryTypes()
        {
            return this.GetDbSet<CategoryType>(); ;
        }

        /// <summary>
        /// Gets all priority types.
        /// </summary>
        /// <returns>List of all priority types (IEnumerable)</returns>
        public IQueryable<PriorityType> GetPriorityTypes()
        {
            return this.GetDbSet<PriorityType>();
        }

        /// <summary>
        /// Gets all status types.
        /// </summary>
        /// <returns>List of all status types (IEnumerable)</returns>
        public IQueryable<StatusType> GetStatusTypes()
        {
            return this.GetDbSet<StatusType>();
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
            /*/// Category Type ID (CC)
            int categoryTypeId = ticket.categoryType_ID;

            /// Category Number (CN)
            var lastTicketInCategory = _ticketList.LastOrDefault(t => t.categoryType_ID == ticket.categoryType_ID);
            int categoryTicketCount = lastTicketInCategory == null ? 1 : lastTicketInCategory.CategoryNumber + 1;

            /// Ticket Number (NN)
            int totalTicketCount = _ticketList.Count + 1;

            /// Set ticket id
            ticket.ticket_ID = $"{categoryTypeId:00}-{categoryTicketCount:00}-{totalTicketCount:00}";*/
            ticket.TicketId = $"CC-CN-{GetAll().Count() + 1}";

            /*/// Update Category Number (CN) index
            lastTicketInCategory = _ticketList.LastOrDefault(t => t.categoryType_ID == ticket.categoryType_ID);
            ticket.CategoryNumber = lastTicketInCategory == null ? 1 : lastTicketInCategory.CategoryNumber + 1;*/
        }
        #endregion
    }
}
