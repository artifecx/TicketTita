using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public TicketService(ITicketRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        /// <summary>
        /// Calls the repository to add a new ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Add(TicketViewModel ticket)
        {
            var newTicket = new Ticket();
            _mapper.Map(ticket, newTicket);
            newTicket.createdDate = DateTime.Now;
            newTicket.updatedDate = new DateTime();
            newTicket.resolvedDate = new DateTime();

            _repository.Add(newTicket);
        }

        /// <summary>
        /// Calls the repository to update an existing ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Update(TicketViewModel ticket)
        {
            var existingTicket = _repository.FindById(ticket.ticket_ID);
            
            if(_repository.FindStatusById(ticket.statusType_ID).statusName.Equals("Closed") && 
                ticket.resolvedDate == DateTime.MinValue)
            {
                ticket.resolvedDate = DateTime.Now;
            }
            else if(!(_repository.FindStatusById(ticket.statusType_ID).statusName.Equals("Closed")) &&
                ticket.resolvedDate != DateTime.MinValue)
            {
                ticket.resolvedDate = new DateTime();
            }
            ticket.updatedDate = DateTime.Now;

            _mapper.Map(ticket, existingTicket);
            _repository.Update(existingTicket);
        }

        /// <summary>
        /// Calls the repository to delete a ticket
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            _repository.Delete(id);
        }


        #region Get Methods        
        /// <summary>
        /// Calls the repository to get a ticket by its id.
        /// Maps the Ticket to the TicketViewModel.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TicketViewModel</returns>
        public TicketViewModel GetTicketById(string id)
        {
            var ticket = _repository.FindById(id);
            return _mapper.Map<TicketViewModel>(ticket);
        }

        /// <summary>
        /// Calls the repository to get all tickets.
        /// Maps the list of Ticket to a list of TicketViewModel.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TicketViewModel> GetAll()
        {
            var tickets = _repository.GetAll();
            return _mapper.Map<IEnumerable<TicketViewModel>>(tickets);
        }

        /// <summary>
        /// Calls the repository to get all category types.
        /// </summary>
        /// <returns>a list of CategoryType (IEnumerable)</returns>
        public IEnumerable<CategoryType> GetCategoryTypes()
        {
            return _repository.GetCategoryTypes();
        }

        /// <summary>
        /// Calls the repository to get all priority types.
        /// </summary>
        /// <returns>a list of PriorityType (IEnumerable)</returns>
        public IEnumerable<PriorityType> GetPriorityTypes()
        {
            return _repository.GetPriorityTypes();
        }

        /// <summary>
        /// Calls the repository to get all status types.
        /// </summary>
        /// <returns>a list of StatusType (IEnumerable)</returns>
        public IEnumerable<StatusType> GetStatusTypes()
        {
            return _repository.GetStatusTypes();
        }
        #endregion
    }
}
