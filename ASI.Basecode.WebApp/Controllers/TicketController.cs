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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TicketController : ControllerBase<TicketController>
    {
        private readonly ITicketService _ticketService;

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
        }

        /// <summary>
        /// Show all tickets
        /// </summary>
        public IActionResult ViewAll()
        {
            var data = _ticketService.GetAll();
            foreach(var d in data)
            {
                d.Attachment = _ticketService.GetAttachmentByTicketId(d.TicketId);
            }
            return View(data);
        }

        /// <summary>
        /// Show ticket details by id
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        //[Authorize]
        [HttpGet]
        public IActionResult ViewTicket(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            if (ticket == null)
            {
                return RedirectToAction("ViewAll");
            }
            return View(ticket);
        }

        /// <summary>
        /// Get method for creating tickets
        /// Initialize the model with category, priority and status types
        /// </summary>
        //[Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new TicketViewModel
            {
                CategoryTypes = _ticketService.GetCategoryTypes(),
                PriorityTypes = _ticketService.GetPriorityTypes(),
                StatusTypes = _ticketService.GetStatusTypes()
            };
            return View(model);
        }

        /// <summary>
        /// Get method for editing tickets
        /// Initialize the ticket with category, priority and status types
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        //[Authorize]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            ticket.PriorityTypes = _ticketService.GetPriorityTypes();
            ticket.StatusTypes = _ticketService.GetStatusTypes();
            ticket.CategoryTypes = _ticketService.GetCategoryTypes();
            return View(ticket);
        }

        /// <summary>
        /// Get method for updating status
        /// </summary>
        [HttpGet]
        public IActionResult UpdateStatus()
        {
            var model = new TicketViewModel
            {
                Tickets = _ticketService.GetAll(),
                CategoryTypes = _ticketService.GetCategoryTypes(),
                PriorityTypes = _ticketService.GetPriorityTypes(),
                StatusTypes = _ticketService.GetStatusTypes()
            };
            return View(model);
        }

        /// <summary>
        /// Get method for updating priority 
        /// </summary>
        [HttpGet]
        public IActionResult UpdatePriority()
        {
            var model = new TicketViewModel
            {
                Tickets = _ticketService.GetAll(),
                CategoryTypes = _ticketService.GetCategoryTypes(),
                PriorityTypes = _ticketService.GetPriorityTypes(),
                StatusTypes = _ticketService.GetStatusTypes()
            };
            return View(model);
        }

        /// <summary>
        /// Get method for deleting tickets
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpGet]
        public IActionResult Delete(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            ticket.Attachment = _ticketService.GetAttachmentByTicketId(id);
            return View(ticket);
        }

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
                    var attachment = new Data.Models.Attachment
                    {
                        AttachmentId = Guid.NewGuid().ToString(),
                        Name = model.File.FileName,
                        Content = stream.ToArray(),
                        Type = model.File.ContentType,
                        UploadedDate = DateTime.Now
                    };
                    model.Attachment = attachment;
                }
            }
            string id = _ticketService.Add(model);

            if(model.Attachment != null)
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
                    var attachment = new Data.Models.Attachment
                    {
                        AttachmentId = Guid.NewGuid().ToString(),
                        Name = model.File.FileName,
                        Content = stream.ToArray(),
                        Type = model.File.ContentType,
                        UploadedDate = DateTime.Now
                    };

                    model.Attachment = attachment;
                }
                
            }
            string id = _ticketService.Update(model);

            /*if (model.Attachment != null)
            {
                model.Attachment.TicketId = id;
                _ticketService.AddAttachment(model.Attachment);
            }*/

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
            if (ticket == null)
            {
                return RedirectToAction("UpdateStatus");
            }

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
            if (ticket == null)
            {
                return RedirectToAction("UpdatePriority");
            }

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
        /// Gets the ticket details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>json containing ticket details</returns>
        [HttpGet]
        public JsonResult GetTicketDetails(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket == null)
            {
                return Json(null);
            }

            var ticketDetails = new
            {
                ticket.TicketId,
                ticket.Subject,
                ticket.IssueDescription,
                ticket.StatusTypeId,
                ticket.CategoryTypeId,
                ticket.PriorityTypeId
            };

            return Json(ticketDetails);
        }

        /// <summary>
        /// Downloads the attachment.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>the file</returns>
        public FileResult DownloadAttachment(string id)
        {
            var attachment = _ticketService.GetAttachmentByTicketId(id);
            if (attachment != null && attachment.Content != null)
            {
                return File(attachment.Content, "application/octet-stream", attachment.Name);
            }

            // TODO: change to proper return type
            return null;
        }
    }
}
