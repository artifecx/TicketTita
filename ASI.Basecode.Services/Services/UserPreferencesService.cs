using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;

namespace ASI.Basecode.Services.Services
{
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly IUserPreferencesRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamService"/> class.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="mapper">The mapper</param>
        /// <param name="logger">The logger</param>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        public UserPreferencesService(
            IUserPreferencesRepository repository,
            ITicketRepository ticketRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IPerformanceReportRepository performanceReportRepository)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        public async Task<UserPreferencesViewModel> GetUserPreferences(string userId)
        {
            var preferences = _repository.GetUserPreferences(userId);
            var categoryTypes = _ticketRepository.GetCategoryTypesAsync();
            var priorityTypes = _ticketRepository.GetPriorityTypesAsync();
            var statusTypes = _ticketRepository.GetStatusTypesAsync();
            await Task.WhenAll(categoryTypes, priorityTypes, statusTypes);

            var model = new UserPreferencesViewModel
            {
                UserId = userId,
                Preferences = preferences,
                CategoryTypes = await categoryTypes,
                PriorityTypes = await priorityTypes,
                StatusTypes = await statusTypes,
            };
            return model;
        }

        public async Task UpdateUserPreferences(UserPreferencesViewModel model)
        {
            var existingPreferences = _repository.GetUserPreferences(model.UserId);
            if (existingPreferences != null && model.Preferences != null)
            {
                foreach (var preference in model.Preferences)
                {
                    existingPreferences[preference.Key] = preference.Value;
                }
                await _repository.UpdateUserPreferences(model.UserId, existingPreferences);
            }
        }
    }
}
