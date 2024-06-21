using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TicketController : ControllerBase<TicketController>
    {
        /// <summary> 
        /// TODO: implement ITicketService in the backend
        /// TODO: replace var with proper data type, implement Ticket model in the backend
        /// </summary>
        private readonly ITicketService _ticketService;
        private readonly IConfiguration _appConfiguration;

        /// <summary>
        /// Initializes a new instance of TicketController
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        public TicketController(
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            ITicketService _ticketService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._ticketService = _ticketService;
            this._appConfiguration = configuration;
        }

        /// <summary>
        /// Create a new ticket
        /// </summary>
        /// <returns>Created response view</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateTicketViewModel ticket)
        {
            if (ModelState.IsValid)
            {
                var ticketModel = _mapper.Map<Ticket>(ticket);

                if (ticket.attachment != null)
                {
                    // TODO: attachment logic
                }

                await _ticketService.CreateTicketAsync(ticketModel);
                return RedirectToAction("View", new { id = ticketModel.Id });
            }

            return View(ticket);
        }

        /// <summary>
        /// Edit an existing ticket
        /// </summary>
        /// <returns>Edited response view</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, EditTicketViewModel ticket)
        {
            if (ModelState.IsValid)
            {
                var ticketModel = await _ticketService.GetTicketByIdAsync(id);
                if (ticketModel == null)
                {
                    return NotFound();
                }

                _mapper.Map(ticket, ticketModel);
                await _ticketService.UpdateTicketAsync(ticketModel);

                return RedirectToAction("View", new { id = ticketModel.Id });
            }

            return View(ticket);
        }

        /// <summary>
        /// Delete a ticket
        /// </summary>
        /// <returns>Redirect to view all tickets</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            await _ticketService.DeleteTicketAsync(id);
            return RedirectToAction("ViewAll");
        }

        /// <summary>
        /// View a ticket
        /// </summary>
        /// <returns>View ticket details</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> ViewTicket(string id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            var ticketViewModel = _mapper.Map<ViewTicketViewModel>(ticket);
            return View(ticketViewModel);
        }

        /// <summary>
        /// View all tickets
        /// </summary>
        /// <returns>View all tickets list</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> ViewAll()
        {
            var tickets = await _ticketService.GetAllTicketsAsync();
            var ticketViewModels = _mapper.Map<IEnumerable<ViewTicketViewModel>>(tickets);
            return View(ticketViewModels);
        }

        /// <summary>
        /// Assign ticket to agent
        /// </summary>
        /// <returns>Assign ticket response</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignTicket(string ticketId, string agentId)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.AssignedAgentId = agentId;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("View", new { id = ticketId });
        }

        /// <summary>
        /// Reassign ticket to another agent
        /// </summary>
        /// <returns>Reassign ticket response</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReassignTicket(string ticketId, string newAgentId)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.AssignedAgentId = newAgentId;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("View", new { id = ticketId });
        }

        /// <summary>
        /// Update ticket status
        /// </summary>
        /// <returns>Update ticket status response</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateTicketStatus(string ticketId, string statusTypeId)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.StatusTypeId = statusTypeId;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("View", new { id = ticketId });
        }

        /// <summary>
        /// Update ticket priority
        /// </summary>
        /// <returns>Update ticket priority response</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateTicketPriority(string ticketId, string priorityTypeId)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.PriorityTypeId = priorityTypeId;
            await _ticketService.UpdateTicketAsync(ticket);

            return RedirectToAction("View", new { id = ticketId });
        }
    }
}
