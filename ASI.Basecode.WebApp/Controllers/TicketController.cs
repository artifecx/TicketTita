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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
            _ticketService = ticketService;
        }

        [Authorize]
        public async Task<IActionResult> ViewAll(string sortBy, string filterBy, string filterValue)
        {
            return await HandleExceptionAsync(async () =>
            {
                await PopulateViewBagAsync();
                var tickets = await _ticketService.GetFilteredAndSortedTicketsAsync(sortBy, filterBy, filterValue);

                ViewData["FilterBy"] = filterBy;
                ViewData["FilterValue"] = filterValue;
                ViewData["SortBy"] = sortBy;

                return View(tickets);
            }, "ViewAll");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewTicket(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null) return RedirectToAction("ViewAll");
                return View(ticket);
            }, "ViewTicket");
        }

        [HttpGet]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> Create()
        {
            return await HandleExceptionAsync(async () => View(await _ticketService.InitializeModelAsync("default")), "Create");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateStatus()
        {
            return await HandleExceptionAsync(async () => View(await _ticketService.InitializeModelAsync("status")), "UpdateStatus");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrAgent")]
        public async Task<IActionResult> UpdatePriority()
        {
            return await HandleExceptionAsync(async () => View(await _ticketService.InitializeModelAsync("priority")), "UpdatePriority");
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AssignTicket()
        {
            return await HandleExceptionAsync(async () => View(await _ticketService.InitializeModelAsync("assign")), "AssignTicket");
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ReassignTicket()
        {
            return await HandleExceptionAsync(async () => View(await _ticketService.InitializeModelAsync("reassign")), "ReassignTicket");
        }

        [HttpGet]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> Edit(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null) return RedirectToAction("ViewAll");
                return View(ticket);
            }, "Edit");
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetTicketDetails(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return Json(null);
                var ticketDetails = await _ticketService.GetTicketDetailsAsync(id);
                if (ticketDetails == null) return Json(null);
                return Json(ticketDetails);
            }, "GetTicketDetails");
        }

        [HttpGet]
        [Authorize]
        public async Task<FileResult> DownloadAttachment(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return null;
                var attachment = await _ticketService.GetAttachmentByTicketIdAsync(id);
                if (attachment == null) return null;
                return File(attachment.Content, "application/octet-stream", attachment.Name);
            }, "DownloadAttachment");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("ViewAll");

                await _ticketService.AddAsync(model, userId);

                TempData["CreateMessage"] = "Created Successfully";
                return RedirectToAction("ViewAll");
            }, "Create");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model == null) return RedirectToAction("ViewAll");
                await _ticketService.UpdateAsync(model);
                return RedirectToAction("ViewAll");
            }, "Edit");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                var ticket = await _ticketService.GetTicketByIdAsync(model.TicketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                ticket.StatusTypeId = model.StatusTypeId;
                await _ticketService.UpdateAsync(ticket);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "UpdateStatus");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdatePriority(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                var ticket = await _ticketService.GetTicketByIdAsync(model.TicketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                ticket.PriorityTypeId = model.PriorityTypeId;
                await _ticketService.UpdateAsync(ticket);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "UpdatePriority");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return Json(new { success = false });
                await _ticketService.DeleteAsync(id);
                return Json(new { success = true });
            }, "Delete");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveAttachment(string ticketId, string attachmentId)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(attachmentId)) return RedirectToAction("ViewAll");
                var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                if (ticket == null) return RedirectToAction("ViewAll");

                await _ticketService.RemoveAttachmentAsync(attachmentId);
                ticket.Attachment = null;
                await _ticketService.UpdateAsync(ticket);
                return RedirectToAction("Edit", new { id = ticketId });
            }, "RemoveAttachment");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignTicket(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model == null || model.Agent == null) return RedirectToAction("ViewAll");
                await _ticketService.AddTicketAssignmentAsync(model);
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "AssignTicket");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReassignTicket(TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model == null) return RedirectToAction("ViewAll");
                var assignment = await _ticketService.GetAssignmentByTicketIdAsync(model.TicketId);
                if (assignment != null && model.Agent.UserId != _ticketService.ExtractAgentId(assignment.AssignmentId))
                {
                    await _ticketService.RemoveAssignmentAsync(assignment.TicketId);
                    if (model.Agent != null && !model.Agent.UserId.Equals("remove"))
                    {
                        await _ticketService.AddTicketAssignmentAsync(model);
                    }
                }
                return RedirectToAction("ViewTicket", new { id = model.TicketId });
            }, "ReassignTicket");
        }

        private async Task PopulateViewBagAsync()
        {
            var usersWithTickets = await _ticketService.GetUserIdsWithTicketsAsync();
            ViewBag.PriorityTypes = (await _ticketService.GetPriorityTypesAsync())
                                    .OrderBy(pt => pt.PriorityTypeId).Select(pt => pt.PriorityName)
                                    .ToList();
            ViewBag.StatusTypes = (await _ticketService.GetStatusTypesAsync())
                                    .OrderBy(st => st.StatusTypeId).Select(st => st.StatusName)
                                    .ToList();
            ViewBag.CategoryTypes = (await _ticketService.GetCategoryTypesAsync())
                                    .OrderBy(ct => ct.CategoryTypeId).Select(ct => ct.CategoryName)
                                    .ToList();
            ViewBag.Users = (await _ticketService.UserGetAllAsync())
                                    .Where(u => u.RoleId == "Employee" && usersWithTickets.Contains(u.UserId))
                                    .OrderBy(u => u.Name).Select(u => u.Name).Distinct().ToList();
        }
    }
}
