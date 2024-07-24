using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
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
        private readonly IUserRepository _userRepository;

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
            IUserRepository userRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IPerformanceReportRepository performanceReportRepository)
        {
            _repository = repository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        
        /// <summary>
        /// Gets the user preferences.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public async Task<UserPreferencesViewModel> GetUserPreferences(string userId)
        {
            var preferences = _repository.GetUserPreferences(userId);
            var categoryTypes = await _ticketRepository.GetCategoryTypesAsync();
            var priorityTypes = await _ticketRepository.GetPriorityTypesAsync();
            var statusTypes = await _ticketRepository.GetStatusTypesAsync();

            var model = new UserPreferencesViewModel
            {
                UserId = userId,
                Preferences = preferences,
                CategoryTypes = categoryTypes,
                PriorityTypes = priorityTypes,
                StatusTypes = statusTypes,
            };
            return model;
        }

        /// <summary>
        /// Updates the user preferences.
        /// </summary>
        /// <param name="model">The model.</param>
        public async Task UpdateUserPreferences(UserPreferencesViewModel model)
        {
            var existingPreferences = _repository.GetUserPreferences(model.UserId);
            if (existingPreferences != null && model.Preferences != null)
            {
                foreach (var preference in model.Preferences)
                {
                    existingPreferences[preference.Key] = preference.Value;
                }
                await _repository.UpdateUserPreferencesAsync(model.UserId, existingPreferences);
            }
        }

        public void UpdateUserPassword(UserPreferencesViewModel model)
        {
            var user = _userRepository.FindById(model.UserId);
            if (user != null)
            {
                bool doesOldPasswordMatchWithCurrent = user.Password == PasswordManager.EncryptPassword(model.oldPassword);
                bool doesNewPasswordMatchWithCurrent = user.Password == PasswordManager.EncryptPassword(model.newPassword);
                if (doesOldPasswordMatchWithCurrent && !doesNewPasswordMatchWithCurrent)
                {
                    user.Password = PasswordManager.EncryptPassword(model.newPassword);
                    _userRepository.Update(user);
                }
                else
                {
                    if (!doesOldPasswordMatchWithCurrent)
                        throw new InvalidOperationException("Old password does not match with the existing password.");

                    if (doesNewPasswordMatchWithCurrent)
                        throw new InvalidOperationException("New password is the same with the existing password.");

                    throw new InvalidOperationException("An error occurred while updating the password. Please try again.");
                }
            }
        }

        public async Task<KeyValuePair<string, string>> GetUserPreferenceByKeyAsync(string userId, string key)
        {
            return await _repository.FindUserPreferenceByKeyAsync(userId, key);
        }
    }
}
