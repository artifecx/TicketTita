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
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TicketController : ControllerBase<TicketController>
    {
        private readonly ITicketService _ticketService;
        private readonly SessionManager _sessionManager;

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
                            ITicketService ticketService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._ticketService = ticketService;
            this._sessionManager = new SessionManager(this._session);
        }

        #region GET Methods
        /// <summary>Show all tickets</summary>
        public IActionResult ViewAll()
        {
            var tickets = _ticketService.GetAll().ToList();
            tickets.ForEach(t => t.Attachment = _ticketService.GetAttachmentByTicketId(t.TicketId));
            return View(tickets);
        }

        /// <summary>
        /// Show ticket details by id
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        public IActionResult ViewTicket(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null) return RedirectToAction("ViewAll");

            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            ticket.TicketAssignment = _ticketService.GetAssignmentByTicketId(id);
            if (ticket.TicketAssignment != null && !string.IsNullOrEmpty(ticket.TicketAssignment.AssignmentId))
            {
                ticket.Agent = _ticketService.GetAgentById(ExtractAgentIdFromAssignmentId(ticket.TicketAssignment.AssignmentId));
            }

            return View(ticket);
        }

        /// <summary>Get method for creating tickets</summary>
        [HttpGet]
        public IActionResult Create() => View(InitializeModel("default"));

        /// <summary>Get method for updating status</summary>
        [HttpGet]
        public IActionResult UpdateStatus() => View(InitializeModel("default"));

        /// <summary>Get method for updating priority</summary>
        [HttpGet]
        public IActionResult UpdatePriority() => View(InitializeModel("default"));

        /// <summary>Get method for adding ticket assignee</summary>
        [HttpGet]
        public IActionResult AssignTicket() => View(InitializeModel("assign"));

        /// <summary>Get method for reassigning ticket assignee</summary>
        [HttpGet]
        public IActionResult ReassignTicket() => View(InitializeModel("reassign"));

        /// <summary>
        /// Get method for editing tickets
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null) return RedirectToAction("ViewAll");

            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            ticket.PriorityTypes = _ticketService.GetPriorityTypes();
            ticket.StatusTypes = _ticketService.GetStatusTypes();
            ticket.CategoryTypes = _ticketService.GetCategoryTypes();
            return View(ticket);
        }

        /// <summary>
        /// Get method for deleting tickets
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        public IActionResult Delete(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null) return RedirectToAction("ViewAll");

            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            return View(ticket);
        }

        /// <summary>
        /// Gets the ticket details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>json containing ticket details</returns>
        [HttpGet]
        public JsonResult GetTicketDetails(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null) return Json(null);

            var assignment = _ticketService.GetAssignmentByTicketId(id);
            if (assignment != null && !string.IsNullOrEmpty(assignment.AssignmentId))
            {
                ticket.Agent = _ticketService.GetAgentById(ExtractAgentIdFromAssignmentId(assignment.AssignmentId));
            }

            var ticketDetails = new
            {
                ticket.TicketId,
                ticket.Subject,
                ticket.IssueDescription,
                ticket.StatusTypeId,
                ticket.CategoryTypeId,
                ticket.PriorityTypeId,
                ticket.Agent
            };

            return Json(ticketDetails);
        }
        #endregion

        #region POST Methods
        /// <summary>
        /// Create a new ticket
        /// </summary>
        /// <returns>Redirect to view all tickets</returns>
        /// Accessible only by client
        [HttpPost]
        public IActionResult Create(TicketViewModel model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    model.File.CopyTo(stream);
                    model.Attachment = new Data.Models.Attachment
                    {
                        AttachmentId = Guid.NewGuid().ToString(),
                        Name = model.File.FileName,
                        Content = stream.ToArray(),
                        Type = model.File.ContentType,
                        UploadedDate = DateTime.Now
                    };
                }
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string id = _ticketService.Add(model, userId);

            if (model.Attachment != null)
            {
                model.Attachment.TicketId = id;
                _ticketService.AddAttachment(model.Attachment);
            }

            return RedirectToAction("ViewAll");
        }

        /// <summary>
        /// Edit an existing ticket
        /// </summary>
        /// <param name="model">the ticket</param>
        /// <returns>Redirect to details of edited ticket</returns>
        /// Accessible only by client
        [HttpPost]
        public IActionResult Edit(TicketViewModel model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    model.File.CopyTo(stream);
                    model.Attachment = new Data.Models.Attachment
                    {
                        AttachmentId = Guid.NewGuid().ToString(),
                        Name = model.File.FileName,
                        Content = stream.ToArray(),
                        Type = model.File.ContentType,
                        UploadedDate = DateTime.Now
                    };
                }

            }
            string id = _ticketService.Update(model);

            if (model.File != null)
            {
                model.Attachment.TicketId = id;
                _ticketService.AddAttachment(model.Attachment);
            }

            return RedirectToAction("ViewAll");
        }

        /// <summary>
        /// Updates the ticket status.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Redirect to ticket details screen</returns>
        [HttpPost]
        public IActionResult UpdateStatus(TicketViewModel model)
        {
            var ticket = _ticketService.GetTicketById(model.TicketId);
            if (ticket == null) return RedirectToAction("UpdateStatus");

            ticket.StatusTypeId = model.StatusTypeId;
            _ticketService.Update(ticket);

            return RedirectToAction("ViewTicket", new { id = model.TicketId });
        }

        /// <summary>
        /// Updates the ticket priority.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Redirect to ticket details screen</returns>
        [HttpPost]
        public IActionResult UpdatePriority(TicketViewModel model)
        {
            var ticket = _ticketService.GetTicketById(model.TicketId);
            if (ticket == null) return RedirectToAction("UpdatePriority");

            ticket.PriorityTypeId = model.PriorityTypeId;
            _ticketService.Update(ticket);

            return RedirectToAction("ViewTicket", new { id = model.TicketId });
        }

        /// <summary>
        /// Delete an existing ticket
        /// </summary>
        /// <param name="model">the ticket</param>
        /// <returns>Redirect to view all tickets</returns>
        /// Accessible only by client
        [HttpPost]
        public IActionResult Delete(TicketViewModel model)
        {
            _ticketService.Delete(model.TicketId);
            return RedirectToAction("ViewAll");
        }

        /// <summary>
        /// Removes the ticket attachment.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="attachmentId">The attachment identifier.</param>
        /// <returns>the edit ticket screen</returns>
        [HttpPost]
        public IActionResult RemoveAttachment(string ticketId, string attachmentId)
        {
            var ticket = _ticketService.GetTicketById(ticketId);
            if (attachmentId != null && ticket != null)
            {
                _ticketService.RemoveAttachment(attachmentId);
                ticket.Attachment = null;
                _ticketService.Update(ticket);
            }
            return RedirectToAction("Edit", new { id = ticketId });
        }

        [HttpPost]
        public IActionResult AssignTicket(TicketViewModel model)
        {
            var ticket = _ticketService.GetTicketById(model.TicketId);
            if (ticket == null) return RedirectToAction("AssignTicket");

            var assignment = _ticketService.GetAssignmentByTicketId(model.TicketId);

            if (assignment == null)
            {
                AssignTicketToAgent(model);
            }

            return RedirectToAction("ViewTicket", new { id = model.TicketId });
        }

        [HttpPost]
        public IActionResult ReassignTicket(TicketViewModel model)
        {
            var ticket = _ticketService.GetTicketById(model.TicketId);
            if (ticket == null) return RedirectToAction("ReassignTicket");

            var assignment = _ticketService.GetAssignmentByTicketId(model.TicketId);

            if (assignment != null && model.Agent.UserId != ExtractAgentIdFromAssignmentId(assignment.AssignmentId))
            {
                _ticketService.RemoveAssignment(assignment.TicketId);

                if (model.Agent != null && !model.Agent.UserId.Equals("remove"))
                {
                    AssignTicketToAgent(model);
                }
            }

            return RedirectToAction("ViewTicket", new { id = model.TicketId });
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Downloads the attachment.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>the file</returns>
        public FileResult DownloadAttachment(string id)
        {
            var attachment = _ticketService.GetAttachmentByTicketId(id);
            return attachment != null ? File(attachment.Content, "application/octet-stream", attachment.Name) : null;
        }

        /// <summary>
        /// Initializes a TicketViewModel to display.
        /// </summary>
        /// <returns>TicketViewModel</returns>
        private TicketViewModel InitializeModel(string type)
        {
            return new TicketViewModel
            {
                Tickets = type.Equals("default") ? _ticketService.GetAll() : type.Equals("assign") 
                            ? _ticketService.GetTickets("unassigned") : _ticketService.GetTickets("assigned"),
                CategoryTypes = _ticketService.GetCategoryTypes(),
                PriorityTypes = _ticketService.GetPriorityTypes(),
                StatusTypes = _ticketService.GetStatusTypes(),
                Agents = _ticketService.GetSupportAgents()
            };
        }

        private void AssignTicketToAgent(TicketViewModel model)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var randomNumber = new Random().Next(1000, 9999);
            var assignmentId = $"{model.Agent.UserId}-{timestamp}-{randomNumber}";

            var assignment = new TicketAssignment
            {
                AssignmentId = assignmentId,
                TeamId = _ticketService.GetTeamByUserId(model.Agent.UserId).TeamId,
                TicketId = model.TicketId,
                AssignedDate = DateTime.Now,
                AdminId = "D56F556E-50A4-4240-A0FF-9A6898B3A03B" //TODO: hardcoded AdminId due to lack of admin login
            };
            model.TicketAssignment = assignment;
            _ticketService.AssignTicket(assignment);
        }

        /// <summary>
        /// Extracts the agent identifier from assignment identifier.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <returns>support agent identifier</returns>
        private string ExtractAgentIdFromAssignmentId(string assignmentId)
        {
            if (assignmentId == null)
                return string.Empty;

            string[] parts = assignmentId.Split('-');

            if (parts.Length >= 5)
            {
                var str = string.Join("-", parts.Take(5));
                return str;
            }
            else
            {
                return assignmentId;
            }
        }
        #endregion
    }
}
