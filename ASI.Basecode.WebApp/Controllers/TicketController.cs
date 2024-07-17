using ASI.Basecode.Data.Models;
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public partial class TicketController : ControllerBase<TicketController>
    {
        private readonly ITicketService _ticketService;
        private readonly IFeedbackService _feedbackService;
        private readonly INotificationService _notificationService;

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
            IFeedbackService feedbackService,
            INotificationService notificationService,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._ticketService = ticketService;
            this._feedbackService = feedbackService;
            this._notificationService = notificationService;
        }

        #region GET methods
        /// <summary>
        /// Shows all tickets
        /// </summary>
        /// <param name="sortBy">User defined, taken from the page</param>
        /// <param name="filterBy">User defined, taken from the page</param>
        /// <param name="filterValue">User defined, taken from the page</param>
        /// <returns>ViewAll page</returns>
        [Authorize]
        public async Task<IActionResult> ViewAll(string sortBy, string filterBy, string filterValue, int pageIndex = 1)
        {
            return await HandleExceptionAsync(async () =>
            {
                await PopulateViewBagAsync();
                var tickets = await _ticketService.GetFilteredAndSortedTicketsAsync(sortBy, filterBy, filterValue, pageIndex, 10);

                ViewData["FilterBy"] = filterBy;
                ViewData["FilterValue"] = filterValue;
                ViewData["SortBy"] = sortBy;

                return View(tickets);
            }, "ViewAll");
        }

        /// <summary>
        /// Shows a specific ticket
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        /// <returns>ViewTicket page</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewTicket(string id, string notificationId, string showModal = null)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null) return RedirectToAction("ViewAll");
                var statusTypes = await _ticketService.GetStatusTypesAsync();
                var users = await _ticketService.UserGetAllAsync();

                ticket.StatusTypes = User.IsInRole("Employee") ? statusTypes.Where(x => x.StatusName != "Resolved" && x.StatusName != "In Progress") : 
                        statusTypes.Where(x => x.StatusName != "Closed" && !(ticket.Agent == null && x.StatusName == "Resolved"));
                ticket.PriorityTypes = await _ticketService.GetPriorityTypesAsync();
                ticket.CategoryTypes = await _ticketService.GetCategoryTypesAsync();
                ticket.Agents = users.Where(x => x.RoleId == "Support Agent");

                if (!string.IsNullOrEmpty(notificationId))
                {
                    _notificationService.MarkNotificationAsRead(notificationId);
                }
                ticket.Comments = ticket.Comments?.OrderByDescending(c => c.PostedDate);
                ViewBag.ShowModal = showModal;
                return View(ticket);
            }, "ViewTicket");
        }
        #endregion GET methods

        #region POST methods
        /// <summary>
        /// Allows the user to create a ticket
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ViewAll page</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if(model != null || string.IsNullOrEmpty(UserId))
                {
                    await _ticketService.AddAsync(model, UserId);
                    TempData["SuccessMessage"] = "New ticket created successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while creating a new ticket. Please try again.";
                return Json(new { success = false });
            }, "Create");
        }

        /// <summary>
        /// Allows the user to edit a ticket
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ViewAll page</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model != null)
                {
                    await _ticketService.UpdateAsync(model,4);
                    TempData["SuccessMessage"] = "Ticket edited successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while editing the ticket. Please try again.";
                return Json(new { success = false });
            }, "Edit");
        }

        /// <summary>
        /// Allows the user to delete a ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(id))
                {
                    await _ticketService.DeleteAsync(id);
                    TempData["SuccessMessage"] = "Ticket deleted successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the ticket. Please try again.";
                return Json(new { success = false });
            }, "Delete");
        }
        #endregion POST methods

        /// <summary>
        /// Populates the view bag with the priority, status, category types and users
        /// Used for dropdowns in the view
        /// </summary>
        private async Task PopulateViewBagAsync()
        {
            var usersWithTickets = await _ticketService.GetUserIdsWithTicketsAsync();
            var priorityTypes = await _ticketService.GetPriorityTypesAsync();
            var statusTypes = await _ticketService.GetStatusTypesAsync();
            var categoryTypes = await _ticketService.GetCategoryTypesAsync();
            var users = await _ticketService.UserGetAllAsync();

            ViewBag.PriorityTypes = priorityTypes.OrderBy(pt => pt.PriorityTypeId).Select(pt => pt.PriorityName).ToList();
            ViewBag.StatusTypes = statusTypes.OrderBy(st => st.StatusTypeId).Select(st => st.StatusName).ToList();
            ViewBag.CategoryTypes = categoryTypes.OrderBy(ct => ct.CategoryTypeId).Select(ct => ct.CategoryName).ToList();
            ViewBag.Users = users.Where(u => u.RoleId == "Employee" && usersWithTickets.Contains(u.UserId)).OrderBy(u => u.Name).Select(u => u.Name).Distinct().ToList();
            ViewBag.PTs = priorityTypes;
            ViewBag.STs = statusTypes;
            ViewBag.CTs = categoryTypes;
        }
    }
}
