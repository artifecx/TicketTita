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
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to user preferences.
    /// </summary>
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly IUserPreferencesRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferencesService"/> class.
        /// </summary>
        /// <param name="repository">The user preferences repository.</param>
        /// <param name="ticketRepository">The ticket repository.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="performanceReportRepository">The performance report repository.</param>
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
        /// Gets the user preferences asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the user preferences view model.</returns>
        public async Task<UserPreferencesViewModel> GetUserPreferencesAsync(string userId)
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
        /// Updates the user preferences asynchronously.
        /// </summary>
        /// <param name="model">The user preferences view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateUserPreferencesAsync(UserPreferencesViewModel model)
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

        /// <summary>
        /// Updates the user password.
        /// </summary>
        /// <param name="model">The user preferences view model.</param>
        /// <exception cref="InvalidOperationException">Thrown when the old password does not match, the new password is the same as the old password, or an error occurs during the update process.</exception>
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
                        throw new InvalidOperationException(Errors.OldPasswordMismatch);

                    if (doesNewPasswordMatchWithCurrent)
                        throw new InvalidOperationException(Errors.NewPasswordSameAsCurrent);

                    throw new InvalidOperationException(Errors.GenericPasswordUpdate);
                }
            }
        }

        /// <summary>
        /// Gets a user preference by key asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="key">The preference key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the key-value pair representing the user preference.</returns>
        public async Task<KeyValuePair<string, string>> GetUserPreferenceByKeyAsync(string userId, string key) =>
            await _repository.FindUserPreferenceByKeyAsync(userId, key);
    }
}
