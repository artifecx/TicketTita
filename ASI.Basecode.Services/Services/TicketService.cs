using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IAdminRepository _adminRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            ITicketRepository repository,
            IAdminRepository adminRepository,
            IUserRepository userRepository,
            IFeedbackRepository feedbackRepository,
            IMapper mapper,
            ILogger<TicketService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _adminRepository = adminRepository;
            _userRepository = userRepository;
            _feedbackRepository = feedbackRepository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<string>> GetUserIdsWithTicketsAsync() => await _repository.GetUserIdsWithTicketsAsync();

        #region Ticket CRUD Operations
        public async Task AddAsync(TicketViewModel ticket, string userId)
        {
            if (ticket != null)
            {
                if (ticket.File != null)
                    await HandleAttachmentAsync(ticket);

                var newTicket = _mapper.Map<Ticket>(ticket);
                newTicket.CreatedDate = DateTime.Now;
                newTicket.UserId = userId;

                AssignTicketProperties(newTicket);

                if (ticket.File != null && ticket.Attachment != null)
                {
                    ticket.Attachment.TicketId = newTicket.TicketId;
                    await AddAttachmentAsync(ticket.Attachment, newTicket);
                }
                else
                {
                    await _repository.AddAsync(newTicket);
                }
            }
            else
            {
                LogError("AddAsync", "Invalid ticket passed.");
            }
        }

        public async Task UpdateAsync(TicketViewModel ticket)
        {
            if (ticket.File != null) await HandleAttachmentAsync(ticket);

            var existingTicket = await _repository.FindByIdAsync(ticket.TicketId);
            if (existingTicket != null)
            {
                ticket.UpdatedDate = DateTime.Now;
                ticket.CategoryType = await _repository.FindCategoryByIdAsync(ticket.CategoryTypeId);
                ticket.PriorityType = await _repository.FindPriorityByIdAsync(ticket.PriorityTypeId);
                ticket.StatusType = await _repository.FindStatusByIdAsync(ticket.StatusTypeId);
                ticket.User = await _repository.UserFindByIdAsync(ticket.UserId);
                
                _mapper.Map(ticket, existingTicket);
                await UpdateTicketDate(existingTicket);

                if (ticket.Attachment != null && ticket.File != null)
                {
                    ticket.Attachment.TicketId = existingTicket.TicketId;
                    await AddAttachmentAsync(ticket.Attachment, existingTicket);
                }
                else
                {
                    await _repository.UpdateAsync(existingTicket);
                }
            }
            else
            {
                LogError("UpdateAsync", "Ticket does not exist.");
            }
        }

        public async Task DeleteAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket != null)
            {
                await RemoveAttachmentByTicketIdAsync(ticket.TicketId);
                await RemoveAssignmentByTicketIdAsync(ticket.TicketId);
                await RemoveFeedbackByTicketIdAsync(ticket.TicketId);
                await _repository.DeleteAsync(ticket);
            }
            else
            {
                LogError("DeleteAsync", "Ticket does not exist.");
            }
        }
        #endregion Ticket CRUD Operations

        #region Ticket Attachment CRUD Operations
        public async Task AddAttachmentAsync(Attachment attachment, Ticket ticket)
        {
            if (attachment != null)
            {
                attachment.Ticket = ticket;
                
                if (attachment.Ticket != null && ticket.Attachments.Count <= 0)
                {
                    ticket.Attachments.Add(attachment);
                    await _repository.AddAttachmentAsync(attachment);
                }
                LogError("AddAttachmentAsync", "Ticket does not exist.");
            }
            LogError("AddAttachmentAsync", "No attachment passed.");
        }

        public async Task RemoveAttachmentAsync(string attachmentId)
        {
            var attachment = await _repository.FindAttachmentByIdAsync(attachmentId);
            var ticket = await _repository.FindByIdAsync(attachment.TicketId);
            if (attachment != null)
            {
                ticket.Attachments.Clear();
                await _repository.RemoveAttachmentAsync(attachment);
            }
        }
        #endregion Ticket Attachment CRUD Operations

        #region Ticket Assignment CRUD Operations
        public async Task AddTicketAssignmentAsync(TicketViewModel model)
        {
            var assignment = await GetAssignmentByTicketIdAsync(model.TicketId);
            if (assignment == null)
            {
                var ticket = await _repository.FindByIdAsync(model.TicketId);
                assignment = await CreateTicketAssignmentAsync(model);
                ticket.TicketAssignment = assignment;
                assignment.Ticket = ticket;
                await _repository.AssignTicketAsync(assignment);
            }
            else
            {
                LogError("AddTicketAssignmentAsync", "Assignment already exists.");
            }
        }

        private async Task<TicketAssignment> CreateTicketAssignmentAsync(TicketViewModel model)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var randomNumber = new Random().Next(1000, 9999);
            var assignmentId = $"{model.Agent.UserId}-{timestamp}-{randomNumber}";
            var currentAdmin = await GetCurrentAdminAsync();
            var teamId = await GetTeamByUserIdAsync(model.Agent.UserId);

            return new TicketAssignment
            {
                AssignmentId = assignmentId,
                TeamId = teamId.TeamId,
                TicketId = model.TicketId,
                AssignedDate = DateTime.Now,
                AdminId = currentAdmin.AdminId,
                Team = await _repository.FindTeamByUserIdAsync(model.Agent.UserId),
                Admin = currentAdmin
            };
        }

        public async Task RemoveAssignmentAsync(string id)
        {
            var assignment = await GetAssignmentByTicketIdAsync(id);
            if (assignment != null)
            {
                var ticket = await _repository.FindByIdAsync(id);
                ticket.TicketAssignment = null;
                await _repository.RemoveAssignmentAsync(assignment);
            }
        }
        #endregion Ticket Assignment CRUD Operations

        #region Ticket Feedback CRUD Operations
        private async Task RemoveFeedbackByTicketIdAsync(string id)
        {
            var feedback = await GetFeedBackByIdAsync(id);
            if (feedback != null) await _repository.FeedbackDeleteAsync(feedback);
        }
        #endregion Ticket Feedback CRUD Operations

        #region Get Methods
        public async Task<IEnumerable<TicketViewModel>> GetAllAsync()
        {
            var tickets = await _repository.GetAllAsync();
            var ticketViewModels = _mapper.Map<IEnumerable<TicketViewModel>>(tickets).ToList();

            return ticketViewModels;
        }

        public async Task<TicketViewModel> GetTicketByIdAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            var mappedTicket = _mapper.Map<TicketViewModel>(ticket);

            return mappedTicket;
        }

        public async Task<IEnumerable<TicketViewModel>> GetUnresolvedTicketsAsync()
        {
            var tickets = (await GetAllAsync()).Where(ticket => ticket.StatusType.StatusName == "Open" || ticket.StatusType.StatusName == "In Progress");
            return tickets;
        }

        public async Task<IEnumerable<TicketViewModel>> GetTicketsByAssignmentStatusAsync(string status, List<string> assignedTicketIds)
        {
            var tickets = await GetUnresolvedTicketsAsync();

            var filteredTickets = status.Equals("unassigned")
                ? tickets.Where(t => !assignedTicketIds.Contains(t.TicketId))
                : tickets.Where(t => assignedTicketIds.Contains(t.TicketId));

            return filteredTickets;
        }

        public async Task<IEnumerable<TicketViewModel>> GetFilteredAndSortedTicketsAsync(string sortBy, string filterBy, string filterValue)
        {
            var tickets = await GetAllAsync();
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userRole.Contains("Employee"))
            {
                tickets = tickets.Where(x => x.UserId == userId);
            }
            else if (userRole.Contains("Support Agent"))
            {
                tickets = tickets.Where(x => x.Agent != null && x.Agent.UserId == userId);
            }

            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                tickets = filterBy.ToLower() switch
                {
                    "priority" => tickets.Where(t => t.PriorityType.PriorityName == filterValue),
                    "status" => tickets.Where(t => t.StatusType.StatusName == filterValue),
                    "category" => tickets.Where(t => t.CategoryType.CategoryName == filterValue),
                    "user" => tickets.Where(t => t.User.Name == filterValue),
                    _ => tickets
                };
            }

            tickets = sortBy switch
            {
                "id_desc" => tickets.OrderByDescending(t => t.TicketId),
                "subject_desc" => tickets.OrderByDescending(t => t.Subject),
                "subject" => tickets.OrderBy(t => t.Subject),
                "status_desc" => tickets.OrderByDescending(t => t.StatusTypeId),
                "status" => tickets.OrderBy(t => t.StatusTypeId),
                "priority_desc" => tickets.OrderByDescending(t => t.PriorityTypeId),
                "priority" => tickets.OrderBy(t => t.PriorityTypeId),
                "category_desc" => tickets.OrderByDescending(t => t.CategoryType.CategoryName),
                "category" => tickets.OrderBy(t => t.CategoryType.CategoryName),
                "user_desc" => tickets.OrderByDescending(t => t.User.Name),
                "user" => tickets.OrderBy(t => t.User.Name),
                "created_desc" => tickets.OrderByDescending(t => t.CreatedDate),
                "created" => tickets.OrderBy(t => t.CreatedDate),
                "updated_desc" => tickets.OrderByDescending(t => t.UpdatedDate),
                "updated" => tickets.OrderBy(t => t.UpdatedDate),
                "resolved_desc" => tickets.OrderByDescending(t => t.ResolvedDate),
                "resolved" => tickets.OrderBy(t => t.ResolvedDate),
                _ => tickets.OrderBy(t => t.TicketId),
            };

            return tickets.AsQueryable();
        }


        public async Task<object> GetTicketDetailsAsync(string id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket != null)
            {
                return new
                {
                    ticket.TicketId,
                    ticket.Subject,
                    ticket.IssueDescription,
                    ticket.StatusTypeId,
                    ticket.CategoryTypeId,
                    ticket.PriorityTypeId,
                    AgentId = ticket.Agent?.UserId,
                };
            }
            return null;
        }

        public async Task<Attachment> GetAttachmentByTicketIdAsync(string id) => await _repository.FindAttachmentByTicketIdAsync(id);

        public async Task<TicketAssignment> GetAssignmentByTicketIdAsync(string id) => await _repository.FindAssignmentByTicketIdAsync(id);

        public async Task<Team> GetTeamByUserIdAsync(string id) => await _repository.FindTeamByUserIdAsync(id);

        public async Task<User> GetAgentByIdAsync(string id) => await _repository.FindAgentByUserIdAsync(id);

        public async Task<IEnumerable<CategoryType>> GetCategoryTypesAsync() => await _repository.GetCategoryTypesAsync();

        public async Task<IEnumerable<PriorityType>> GetPriorityTypesAsync() => await _repository.GetPriorityTypesAsync();

        public async Task<IEnumerable<StatusType>> GetStatusTypesAsync() => await _repository.GetStatusTypesAsync();

        public async Task<IEnumerable<User>> GetSupportAgentsAsync() => await _repository.GetSupportAgentsAsync();

        public async Task<IEnumerable<TicketAssignment>> GetTicketAssignmentsAsync() => await _repository.GetTicketAssignmentsAsync();
        #endregion Get Methods

        #region Utility Methods
        public async Task<TicketViewModel> InitializeModelAsync(string type)
        {
            var assignedTicketIds = (await GetTicketAssignmentsAsync()).Select(ta => ta.TicketId).ToList();
            var tickets = type switch
            {
                "status" => (await GetAllAsync()).Where(ticket => !ticket.StatusType.StatusName.Contains("Resolved")),
                "assign" => (await GetTicketsByAssignmentStatusAsync("unassigned", assignedTicketIds)),
                "priority" => await GetUnresolvedTicketsAsync(),
                "reassign" => (await GetTicketsByAssignmentStatusAsync("assigned", assignedTicketIds)),
                _ => null
            };

            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tickets != null)
            {
                tickets = userRole.Contains("Support Agent") ? tickets.Where(x => x.Agent != null && x.Agent.UserId == userId)
                        : userRole.Contains("Employee") ? tickets.Where(x => x.UserId == userId) : tickets;
            }

            return new TicketViewModel
            {
                Tickets = !type.Contains("default") ? tickets : null,
                CategoryTypes = await GetCategoryTypesAsync(),
                PriorityTypes = await GetPriorityTypesAsync(),
                StatusTypes = !userRole.Contains("Employee") ? await GetStatusTypesAsync() : (await GetStatusTypesAsync()).Where(x => !x.StatusName.Contains("Resolved")),
                Agents = !type.Contains("default") ? await GetSupportAgentsAsync() : null
            };
        }

        public string ExtractAgentId(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId)) return string.Empty;

            string[] parts = assignmentId.Split('-');
            return parts.Length >= 5 ? string.Join("-", parts.Take(5)) : assignmentId;
        }

        private async Task HandleAttachmentAsync(TicketViewModel ticket)
        {
            _logger.LogInformation("=======Ticket Service : HandleAttachmentAsync Started=======");
            var allowedFileTypesString = Resources.Views.FileValidation.AllowedFileTypes;
            var allowedFileTypes = new HashSet<string>(allowedFileTypesString.Split(','), StringComparer.OrdinalIgnoreCase);
            long maxFileSize = Convert.ToInt32(Resources.Views.FileValidation.MaxFileSizeMB) * 1024 * 1024;

            if (ticket.File != null && ticket.File.Length > 0)
            {
                if (allowedFileTypes.Contains(ticket.File.ContentType) && ticket.File.Length <= maxFileSize)
                {
                    using (var stream = new MemoryStream())
                    {
                        await ticket.File.CopyToAsync(stream);
                        ticket.Attachment = new Attachment
                        {
                            AttachmentId = Guid.NewGuid().ToString(),
                            Name = ticket.File.FileName,
                            Content = stream.ToArray(),
                            Type = ticket.File.ContentType,
                            UploadedDate = DateTime.Now
                        };
                    }
                }
                else
                {
                    _logger.LogError("File type not allowed or file size exceeds the limit."); 
                }
            }
            _logger.LogInformation("=======Ticket Service : HandleAttachmentAsync Ended=======");
        }

        private async Task UpdateTicketDate(Ticket ticket)
        {
            var status = await _repository.FindStatusByIdAsync(ticket.StatusTypeId);

            if (status != null && (status.StatusName.Equals("Closed") || status.StatusName.Equals("Resolved")))
            {
                ticket.ResolvedDate = ticket.ResolvedDate ?? DateTime.Now;
            }
            else
            {
                ticket.ResolvedDate = null;
            }
        }

        private void AssignTicketProperties(Ticket ticket)
        {
            var tickets = GetAllAsync().Result;
            string CC = ticket.CategoryTypeId;
            int CN = tickets.Count(t => t.CategoryTypeId == ticket.CategoryTypeId);
            int NN = tickets.Count();

            ticket.TicketId = $"{CC}-{CN + 1:00}-{NN + 1:00}";

            ticket.StatusTypeId ??= "S1";
            ticket.CategoryType = GetCategoryTypesAsync().Result.Single(x => x.CategoryTypeId == ticket.CategoryTypeId);
            ticket.PriorityType = GetPriorityTypesAsync().Result.Single(x => x.PriorityTypeId == ticket.PriorityTypeId);
            ticket.StatusType = GetStatusTypesAsync().Result.Single(x => x.StatusTypeId == ticket.StatusTypeId);
            ticket.User = _repository.UserFindByIdAsync(ticket.UserId).Result;
            ticket.User.Tickets.Add(ticket);
        }
        
        private async Task RemoveAttachmentByTicketIdAsync(string id)
        {
            var attachment = await GetAttachmentByTicketIdAsync(id);
            if (attachment != null)
            {
                await _repository.RemoveAttachmentAsync(attachment);
            }
        }

        private async Task RemoveAssignmentByTicketIdAsync(string id)
        {
            var assignment = await GetAssignmentByTicketIdAsync(id);
            if (assignment != null)
            {
                await _repository.RemoveAssignmentAsync(assignment);
            }
        }

        private async Task<Admin> GetCurrentAdminAsync()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext.User;
            if (claimsPrincipal == null || !claimsPrincipal.Identity.IsAuthenticated)
            {
                LogError("GetCurrentAdminAsync", "ClaimsPrincipal is null or not authenticated.");
                return null;
            }

            var adminId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
            {
                LogError("GetCurrentAdminAsync", "AdminId is null or empty.");
                return null;
            }

            return await _repository.AdminFindByIdAsync(adminId);
        }

        private async Task<Feedback> GetFeedBackByIdAsync(string id)
        {
            return await _repository.FeedbackFindByTicketIdAsync(id);
        }
        #endregion Utility Methods

        #region Logging methods
        private void LogError(string methodName, string errorMessage)
        {
            _logger.LogError($"Ticket Service {methodName} : {errorMessage}");
        }
        #endregion

        public async Task<IEnumerable<User>> UserGetAllAsync()
        {
            return await _repository.UserGetAllAsync();
        }
    }
}
