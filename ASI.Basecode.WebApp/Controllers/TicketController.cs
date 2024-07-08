using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TicketController : ControllerBase<TicketController>
    {
        private readonly ITicketService _ticketService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketController"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="ticketService">The ticket service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
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
        }

        /// <summary>Show all tickets</summary>
        [Authorize]
        public IActionResult ViewAll(string sortOrder)
        {
            return HandleException(() =>
            {
                var tickets = _ticketService.GetAll();
                var UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (User.IsInRole("Employee"))
                {
                    tickets = tickets.Where(x => x.UserId == UserId).ToList();
                } 
                else if(User.IsInRole("Support Agent"))
                {
                    tickets = tickets.Where(x => x.Agent?.UserId == UserId).ToList();
                }

                ViewData["CurrentSort"] = sortOrder;
                switch (sortOrder)
                {
                    case "status_desc":
                        tickets = tickets.OrderByDescending(t => t.StatusTypeId).ToList();
                        break;
                    case "status":
                        tickets = tickets.OrderBy(t => t.StatusTypeId).ToList();
                        break;
                    case "priority_desc":
                        tickets = tickets.OrderByDescending(t => t.PriorityTypeId).ToList();
                        break;
                    case "priority":
                        tickets = tickets.OrderBy(t => t.PriorityTypeId).ToList();
                        break;
                    case "category_desc":
                        tickets = tickets.OrderByDescending(t => t.CategoryType.CategoryName).ToList();
                        break;
                    case "category":
                        tickets = tickets.OrderBy(t => t.CategoryType.CategoryName).ToList();
                        break;
                    case "user_desc":
                        tickets = tickets.OrderByDescending(t => t.User.Name);
                        break;
                    case "user":
                        tickets = tickets.OrderBy(t => t.User.Name);
                        break;
                    default:
                        tickets = tickets.OrderBy(t => t.TicketId);
                        break;
                }

                return View(tickets);
            }, "ViewAll");
        }

        #region GET Methods
        /// <summary>
        /// Show ticket details by id
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        [Authorize]
        public IActionResult ViewTicket(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var ticket = _ticketService.GetTicketById(id);
                if (ticket == null) return RedirectToAction("ViewAll");
                return View(ticket);
            }, "ViewTicket");
        }

        /// <summary>Get method for creating tickets</summary>
        [HttpGet]
        [Authorize(Policy = "Employee")]
        public IActionResult Create() => HandleException(() => View(_ticketService.InitializeModel("default")), "Create");

        /// <summary>Get method for updating status</summary>
        [HttpGet]
        [Authorize]
        public IActionResult UpdateStatus() => HandleException(() => View(_ticketService.InitializeModel("status")), "UpdateStatus");

        /// <summary>Get method for updating priority</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOrAgent")]
        public IActionResult UpdatePriority() => HandleException(() => View(_ticketService.InitializeModel("default")), "UpdatePriority");

        /// <summary>Get method for adding ticket assignee</summary>
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult AssignTicket() => HandleException(() => View(_ticketService.InitializeModel("assign")), "AssignTicket");

        /// <summary>Get method for reassigning ticket assignee</summary>
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public IActionResult ReassignTicket() => HandleException(() => View(_ticketService.InitializeModel("reassign")), "ReassignTicket");

        /// <summary>
        /// Get method for editing tickets
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        [Authorize(Policy = "Employee")]
        public IActionResult Edit(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var ticket = _ticketService.GetTicketById(id);
                if (ticket == null) return RedirectToAction("ViewAll");
                return View(ticket);
            }, "Edit");
        }

        /// <summary>
        /// Gets the ticket details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json containing ticket details</returns>
        [HttpGet]
        [Authorize]
        public JsonResult GetTicketDetails(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return Json(null);
                var ticketDetails = _ticketService.GetTicketDetails(id);
                if (ticketDetails == null) return Json(null);
                return Json(ticketDetails);
            }, "GetTicketDetails");
        }

        /// <summary>
        /// Downloads the attachment.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>The file</returns>
        [HttpGet]
        [Authorize]
        public FileResult DownloadAttachment(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return null;
                var attachment = _ticketService.GetAttachmentByTicketId(id);
                if (attachment == null) return null;
                return File(attachment.Content, "application/octet-stream", attachment.Name);
            }, "DownloadAttachment");
        }
        #endregion GET Methods

        #region POST Methods
        /// <summary>
        /// Create a new ticket
        /// </summary>
        /// <returns>View all tickets screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult Create(TicketViewModel model)
        {
            return HandleException(() =>
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("ViewAll");

                _ticketService.Add(model, userId);

                TempData["CreateMessage"] = "Created Successfully";
                return RedirectToAction("ViewAll");
            }, "Create");
        }

        /// <summary>
        /// Edit an existing ticket
        /// </summary>
        /// <param name="model">the ticket</param>
        /// <returns>View all tickets screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult Edit(TicketViewModel model)
        {
            return HandleException(() =>
            {
                if (model == null) return RedirectToAction("ViewAll");
                _ticketService.Update(model);
                return RedirectToAction("ViewAll");
            }, "Edit");
        }

        /// <summary>
        /// Updates the ticket status.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Ticket details screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult UpdateStatus(TicketViewModel model)
        {
            return HandleException(() =>
            {
                var ticket = _ticketService.GetTicketById(model.TicketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                ticket.StatusTypeId = model.StatusTypeId;
                _ticketService.Update(ticket);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "UpdateStatus");
        }

        /// <summary>
        /// Updates the ticket priority.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Ticket details screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult UpdatePriority(TicketViewModel model)
        {
            return HandleException(() =>
            {
                var ticket = _ticketService.GetTicketById(model.TicketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                ticket.PriorityTypeId = model.PriorityTypeId;
                _ticketService.Update(ticket);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "UpdatePriority");
        }

        /// <summary>
        /// Delete an existing ticket
        /// </summary>
        /// <param name="model">the ticket</param>
        /// <returns>Deletion success status (bool)</returns>
        [HttpPost]
        [Authorize]
        public IActionResult Delete(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return Json(new { success = false });
                _ticketService.Delete(id);
                return Json(new { success = true });
            }, "Delete");
        }

        /// <summary>
        /// Removes the ticket attachment.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="attachmentId">The attachment identifier.</param>
        /// <returns>Edit ticket screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult RemoveAttachment(string ticketId, string attachmentId)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(attachmentId)) return RedirectToAction("ViewAll");
                var ticket = _ticketService.GetTicketById(ticketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                _ticketService.RemoveAttachment(attachmentId);
                ticket.Attachment = null;
                _ticketService.Update(ticket);
                return RedirectToAction("Edit", new { id = ticketId });
            }, "RemoveAttachment");
        }

        /// <summary>
        /// Assigns the ticket.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Ticket details screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult AssignTicket(TicketViewModel model)
        {
            return HandleException(() =>
            {
                if (model == null || model.Agent == null) return RedirectToAction("ViewAll");
                _ticketService.AddTicketAssignment(model);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "AssignTicket");
        }

        /// <summary>
        /// Reassigns the ticket.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Ticket details screen</returns>
        [HttpPost]
        [Authorize]
        public IActionResult ReassignTicket(TicketViewModel model)
        {
            return HandleException(() =>
            {
                if (model == null) return RedirectToAction("ViewAll");
                var assignment = _ticketService.GetAssignmentByTicketId(model.TicketId);
                if (assignment != null && model.Agent.UserId != _ticketService.ExtractAgentId(assignment.AssignmentId))
                {
                    _ticketService.RemoveAssignment(assignment.TicketId);
                    if (model.Agent != null && !model.Agent.UserId.Equals("remove"))
                    {
                        _ticketService.AddTicketAssignment(model);
                    }
                }
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "ReassignTicket");
        }
        #endregion POST Methods
    }
}
