/*using ASI.Basecode.Data.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// fetches and sets the category, priority, and status types to prevent multiple calls to the database
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
        /// foreach will set the navigation properties for each ticket: CategoryType, PriorityType, StatusType
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
        /// Adds a new ticket to the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Add(Ticket ticket)
        {
            AssignTicketProperties(ticket);

            this.GetDbSet<Ticket>().Add(ticket);
            UnitOfWork.SaveChanges();

            return ticket.TicketId;
        }

        /// <summary>
        /// Adds a new attachment to the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void AddAttachment(Attachment attachment)
        {
            attachment.Ticket = FindById(attachment.TicketId);
            this.GetDbSet<Attachment>().Add(attachment);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Updates an existing ticket in the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Update(Ticket ticket)
        {
            SetNavigation(ticket);

            this.GetDbSet<Ticket>().Update(ticket);
            UnitOfWork.SaveChanges();

            return ticket.TicketId;
        }

        /// <summary>
        /// Deletes the specified ticket found using the identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var existingTicket = FindById(id);
            var existingAttachment = this.GetDbSet<Attachment>().Where(x => x.TicketId.Equals(id)).FirstOrDefault();
            if (existingTicket != null)
            {
                if(existingAttachment != null)
                {
                    this.GetDbSet<Attachment>().Remove(existingAttachment);
                }
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
        /// Finds an attachment by ticket identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>an attachment</returns>
        public Attachment FindAttachmentByTicketId(string id)
        {
            return this.GetDbSet<Attachment>().Where(x => x.TicketId.Equals(id)).FirstOrDefault();
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
        /// <returns>A collection of all category types (IQueryable)</returns>
        public IQueryable<CategoryType> GetCategoryTypes()
        {
            return this.GetDbSet<CategoryType>(); ;
        }

        /// <summary>
        /// Gets all priority types.
        /// </summary>
        /// <returns>A collection of all priority types (IQueryable)</returns>
        public IQueryable<PriorityType> GetPriorityTypes()
        {
            return this.GetDbSet<PriorityType>();
        }

        /// <summary>
        /// Gets all status types.
        /// </summary>
        /// <returns>A collection of all status types (IQueryable)</returns>
        public IQueryable<StatusType> GetStatusTypes()
        {
            return this.GetDbSet<StatusType>();
        }

        /// <summary>
        /// Assigns the following ticket properties: TicketId, CategoryType, PriorityType, StatusType
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// 
        /// Ticket ID format: CC-CN-NN
        /// CC = Category Type ID
        /// CN = Category Number (how many tickets have been created for a category)
        /// NN = Ticket Number (total number of tickets created)
        private void AssignTicketProperties(Ticket ticket)
        {
            /// TicketId is reused if an entry is deleted and a new one has the same category as deleted entry
            /// not scalable if database has many entries
            // TODO: revisit this logic
            int CC = Convert.ToInt32(ticket.CategoryTypeId);
            int CN = GetAll().Count(t => t.CategoryTypeId == ticket.CategoryTypeId);
            int NN = GetAll().Count();

            ticket.TicketId = $"{CC:00}-{CN + 1:00}-{NN + 1:00}";

            ticket.StatusTypeId = ticket.StatusTypeId ?? "1";
            SetNavigation(ticket);
        }


        /// <summary>
        /// Sets the navigation properties for a ticket: CategoryType, PriorityType, StatusType
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        private void SetNavigation(Ticket ticket)
        {
            ticket.CategoryType = _categoryTypes.Single(x => x.CategoryTypeId == ticket.CategoryTypeId);
            ticket.PriorityType = _priorityTypes.Single(x => x.PriorityTypeId == ticket.PriorityTypeId);
            ticket.StatusType = _statusTypes.Single(x => x.StatusTypeId == ticket.StatusTypeId);
        }
        #endregion
    }
}
*/