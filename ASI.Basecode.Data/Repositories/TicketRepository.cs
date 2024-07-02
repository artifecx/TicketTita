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
        private readonly List<TicketAssignment> _ticketAssignments;

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
            _ticketAssignments = GetTicketAssignments().ToList();
        }

        /// <summary>
        /// Gets all tickets.
        /// </summary>
        /// <returns>a list of all tickets (IEnumerable)</returns>
        /// foreach will set the navigation properties for each ticket: CategoryType, PriorityType, StatusType
        public IQueryable<Ticket> GetAll()
        {
            var tickets = this.GetDbSet<Ticket>();
            foreach (var ticket in tickets)
            {
                SetNavigationProperties(ticket);
                ticket.TicketAssignments = _ticketAssignments.Where(x => x.TicketId == ticket.TicketId).ToList(); //TODO: check if this is necessary
            }
            return tickets;
        }

        public IQueryable<Ticket> GetTickets(string type)
        {
            var assignedTicketIds = _ticketAssignments.Select(ta => ta.TicketId).ToList();

            var tickets = type.Equals("unassigned")
                ? this.GetDbSet<Ticket>().Where(t => !assignedTicketIds.Contains(t.TicketId))
                : this.GetDbSet<Ticket>().Where(t => assignedTicketIds.Contains(t.TicketId));

            foreach (var ticket in tickets)
            {
                SetNavigationProperties(ticket);
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
        /// Updates an existing ticket in the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Update(Ticket ticket)
        {
            SetNavigationProperties(ticket);

            this.GetDbSet<Ticket>().Update(ticket);
            UnitOfWork.SaveChanges();

            return ticket.TicketId;
        }

        /// <summary>
        /// Deletes the specified ticket found using the identifier.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        public void Delete(Ticket ticket)
        {
            var existingAttachment = FindAttachmentByTicketId(ticket.TicketId);
            if (existingAttachment != null)
            {
                this.GetDbSet<Attachment>().Remove(existingAttachment);
            }

            var existingAssignment = FindAssignmentByTicketId(ticket.TicketId);
            if (existingAssignment != null)
            {
                this.GetDbSet<TicketAssignment>().Remove(existingAssignment);
            }

            this.GetDbSet<Ticket>().Remove(ticket);
            UnitOfWork.SaveChanges();
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
        /// Removes the ticket attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        public void RemoveAttachment(Attachment attachment)
        {
            this.GetDbSet<Attachment>().Remove(attachment);
            UnitOfWork.SaveChanges();
        }

        public void AssignTicket(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Add(assignment);
            UnitOfWork.SaveChanges();
        }

        public void RemoveAssignment(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Remove(assignment);
            UnitOfWork.SaveChanges();
        }


        #region Helper Methods
        /// <summary>
        /// Finds a ticket by identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>a Ticket</returns>
        public Ticket FindById(string id)
        {
            return this.GetDbSet<Ticket>().FirstOrDefault(x => x.TicketId == id);
        }

        /// <summary>
        /// Finds an attachment by ticket identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>an attachment</returns>
        public Attachment FindAttachmentByTicketId(string id)
        {
            return this.GetDbSet<Attachment>().FirstOrDefault(x => x.TicketId == id);
        }

        /// <summary>
        /// Finds the attachment by its identifier.
        /// </summary>
        /// <param name="id">The attachment identifier.</param>
        /// <returns>an attachment</returns>
        public Attachment FindAttachmentById(string id)
        {
            return this.GetDbSet<Attachment>().FirstOrDefault(x => x.AttachmentId == id);
        }

        public TicketAssignment FindAssignmentByTicketId(string id) {
            return this.GetDbSet<TicketAssignment>().FirstOrDefault(x => x.TicketId == id);
        }

        public Team FindTeamByUserId(string id)
        {
            var memberOf = this.GetDbSet<TeamMember>().FirstOrDefault(x => x.UserId == id);
            return this.GetDbSet<Team>().FirstOrDefault(x => x.TeamId == memberOf.TeamId);
        }

        public User FindAgentByUserId(string id)
        {
            return this.GetDbSet<User>().FirstOrDefault(x => x.UserId == id);
        }

        /// <summary>
        /// Finds the category by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a category type (CategoryType)</returns>
        public CategoryType FindCategoryById(string id)
        {
            return this.GetDbSet<CategoryType>().FirstOrDefault(x => x.CategoryTypeId == id);
        }

        /// <summary>
        /// Finds the priority by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a priority type (PriorityType)</returns>
        public PriorityType FindPriorityById(string id)
        {
            return this.GetDbSet<PriorityType>().FirstOrDefault(x => x.PriorityTypeId == id);
        }

        /// <summary>
        /// Finds the status by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>a status type (StatusType)</returns>
        public StatusType FindStatusById(string id)
        {
            return this.GetDbSet<StatusType>().FirstOrDefault(x => x.StatusTypeId == id);
        }

        /// <summary>
        /// Gets all category types.
        /// </summary>
        /// <returns>A collection of all category types (IQueryable)</returns>
        public IQueryable<CategoryType> GetCategoryTypes()
        {
            return this.GetDbSet<CategoryType>();
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

        public IQueryable<User> GetSupportAgents()
        {
            return this.GetDbSet<User>().Where(x => x.RoleId == "Support Agent");
        }

        public IQueryable<TicketAssignment> GetTicketAssignments()
        {
            return this.GetDbSet<TicketAssignment>();
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

            ticket.StatusTypeId ??= "1";
            SetNavigationProperties(ticket);
        }


        /// <summary>
        /// Sets the navigation properties for a ticket: CategoryType, PriorityType, StatusType
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        private void SetNavigationProperties(Ticket ticket)
        {
            ticket.CategoryType = _categoryTypes.Single(x => x.CategoryTypeId == ticket.CategoryTypeId);
            ticket.PriorityType = _priorityTypes.Single(x => x.PriorityTypeId == ticket.PriorityTypeId);
            ticket.StatusType = _statusTypes.Single(x => x.StatusTypeId == ticket.StatusTypeId);
        }
        #endregion
    }
}