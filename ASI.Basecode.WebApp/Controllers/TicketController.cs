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
        /// Show all tickets
        /// </summary>
        public IActionResult ViewAll()
        {
            var data = _ticketService.GetAll();
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
            ticket.CategoryTypes = _ticketService.GetCategoryTypes();
            ticket.PriorityTypes = _ticketService.GetPriorityTypes();
            ticket.StatusTypes = _ticketService.GetStatusTypes();
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
            if(model.File != null && model.File.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    model.File.CopyTo(stream);
                    model.attachment = new Data.Models.Attachment
                    {
                        fileContent = stream.ToArray(),
                        fileName = model.File.FileName,
                        contentType = model.File.ContentType,
                        uploadedDate = DateTime.Now
                    };
                }
            }
            
            _ticketService.Add(model);
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
                    model.attachment = new Data.Models.Attachment
                    {
                        fileContent = stream.ToArray(),
                        fileName = model.File.FileName,
                        contentType = model.File.ContentType,
                        uploadedDate = DateTime.Now
                    };
                }
            }

            _ticketService.Update(model);
            return RedirectToAction("ViewAll");
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
            _ticketService.Delete(model.ticket_ID);
            return RedirectToAction("ViewAll");
        }


        /// <summary>
        /// Downloads the attachment.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>the file</returns>
        public FileResult DownloadAttachment(string id)
        {
            var ticket = _ticketService.GetTicketById(id);
            if (ticket != null && ticket.attachment != null)
            {
                return File(ticket.attachment.fileContent, "application/octet-stream", ticket.attachment.fileName);
            }

            return null;
        }
    }
}
