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
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region Ticket Service Methods
        /// <summary>
        /// Gets all tickets.
        /// </summary>
        /// <returns>a list of all tickets (IEnumerable)</returns>
        /// foreach will set the navigation properties for each ticket: CategoryType, PriorityType, StatusType
        public IQueryable<Ticket> GetAll() => this.GetDbSet<Ticket>();

        public IQueryable<Ticket> GetTickets(string type, List<string> assignedTicketIds)
        {
            return type.Equals("unassigned")
                ? this.GetDbSet<Ticket>().Where(t => !assignedTicketIds.Contains(t.TicketId))
                : this.GetDbSet<Ticket>().Where(t => assignedTicketIds.Contains(t.TicketId));
        }

        /// <summary>
        /// Adds a new ticket to the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Add(Ticket ticket)
        {
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
            this.GetDbSet<Ticket>().Update(ticket);
            UnitOfWork.SaveChanges();
            return ticket.TicketId;
        }

        /// <summary>
        /// Deletes the specified ticket found in the database using the identifier.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        public void Delete(Ticket ticket)
        {
            this.GetDbSet<Ticket>().Remove(ticket);
            UnitOfWork.SaveChanges();
        }
        #endregion Ticket Service Methods

        #region Attachment Service Methods
        /// <summary>
        /// Adds a new attachment to the database.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void AddAttachment(Attachment attachment)
        {
            this.GetDbSet<Attachment>().Add(attachment);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Removes the ticket attachment from the database.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        public void RemoveAttachment(Attachment attachment)
        {
            this.GetDbSet<Attachment>().Remove(attachment);
            UnitOfWork.SaveChanges();
        }
        #endregion Attachment Service Methods

        #region Ticket Assignment Service Methods        
        /// <summary>
        /// Adds a new TicketAssignment to the database.
        /// </summary>
        /// <param name="assignment">The assignment.</param>
        public void AssignTicket(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Add(assignment);
            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Removes the TicketAssignment from the database.
        /// </summary>
        /// <param name="assignment">The assignment.</param>
        public void RemoveAssignment(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Remove(assignment);
            UnitOfWork.SaveChanges();
        }
        #endregion Ticket Assignment Service Methods


        #region Find Methods
        /// <summary>
        /// Finds a ticket in the database using its identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>Ticket</returns>
        public Ticket FindById(string id) => this.GetDbSet<Ticket>().FirstOrDefault(x => x.TicketId == id);

        /// <summary>
        /// Finds an attachment in the database using its identifier.
        /// </summary>
        /// <param name="id">The attachment identifier.</param>
        /// <returns>Attachment</returns>
        public Attachment FindAttachmentById(string id) => this.GetDbSet<Attachment>().FirstOrDefault(x => x.AttachmentId == id);

        /// <summary>
        /// Finds an attachment in the database using the ticket identifier.
        /// </summary>
        /// <param name="id">ticket_ID</param>
        /// <returns>Attachment</returns>
        public Attachment FindAttachmentByTicketId(string id) => this.GetDbSet<Attachment>().FirstOrDefault(x => x.TicketId == id);

        /// <summary>
        /// Finds an assignment in the database using the ticket identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TicketAssignment</returns>
        public TicketAssignment FindAssignmentByTicketId(string id) => this.GetDbSet<TicketAssignment>().FirstOrDefault(x => x.TicketId == id);

        /// <summary>
        /// Finds the Team through the user identifier of a TeamMember.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Team</returns>
        public Team FindTeamByUserId(string id)
        {
            var memberOf = this.GetDbSet<TeamMember>().FirstOrDefault(x => x.UserId == id);
            return this.GetDbSet<Team>().FirstOrDefault(x => x.TeamId == memberOf.TeamId);
        }

        /// <summary>
        /// Finds the agent by user identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>User</returns>
        public User FindAgentByUserId(string id) => this.GetDbSet<User>().FirstOrDefault(x => x.UserId == id);

        /// <summary>
        /// Finds the category by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>CategoryType</returns>
        public CategoryType FindCategoryById(string id) => this.GetDbSet<CategoryType>().FirstOrDefault(x => x.CategoryTypeId == id);

        /// <summary>
        /// Finds the priority by itsidentifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>PriorityType</returns>
        public PriorityType FindPriorityById(string id) => this.GetDbSet<PriorityType>().FirstOrDefault(x => x.PriorityTypeId == id);

        /// <summary>
        /// Finds the status by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>StatusType</returns>
        public StatusType FindStatusById(string id) => this.GetDbSet<StatusType>().FirstOrDefault(x => x.StatusTypeId == id);

        /// <summary>
        /// Gets all category types.
        /// </summary>
        /// <returns>A collection of all CategoryType (IQueryable)</returns>
        public IQueryable<CategoryType> GetCategoryTypes() => this.GetDbSet<CategoryType>();

        /// <summary>
        /// Gets all priority types.
        /// </summary>
        /// <returns>A collection of all PriorityType (IQueryable)</returns>
        public IQueryable<PriorityType> GetPriorityTypes() => this.GetDbSet<PriorityType>();

        /// <summary>
        /// Gets all status types.
        /// </summary>
        /// <returns>A collection of all StatusType (IQueryable)</returns>
        public IQueryable<StatusType> GetStatusTypes() => this.GetDbSet<StatusType>();

        /// <summary>
        /// Gets the support agents.
        /// </summary>
        /// <returns>User with support agent role</returns>
        public IQueryable<User> GetSupportAgents() => this.GetDbSet<User>().Where(x => x.RoleId == "Support Agent");

        /// <summary>
        /// Gets the ticket assignments.
        /// </summary>
        /// <returns>A TicketAssignment</returns>
        public IQueryable<TicketAssignment> GetTicketAssignments() => this.GetDbSet<TicketAssignment>();
        #endregion
    }
}