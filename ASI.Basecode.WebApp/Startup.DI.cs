﻿using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp
{
    // Other services configuration
    internal partial class StartupConfigurer
    {
        /// <summary>
        /// Configures the other services.
        /// </summary>
        private void ConfigureOtherServices()
        {
            // Framework
            this._services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            this._services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Common
            this._services.AddScoped<TokenProvider>();
            this._services.TryAddSingleton<TokenProviderOptionsFactory>();
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            this._services.TryAddSingleton<TokenValidationParametersFactory>();
            this._services.AddScoped<IAccountService, AccountService>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<ITicketService, TicketService>();
            this._services.AddScoped<IFeedbackService, FeedbackService>();
            this._services.AddScoped<ITeamService, TeamService>();
            this._services.AddScoped<IUserService, UserService>();
            this._services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
            this._services.AddScoped<INotificationService, NotificationService>();
            this._services.AddScoped<IUserPreferencesService, UserPreferencesService>();
            this._services.AddScoped<IActivityLogService, ActivityLogService>();
            this._services.AddScoped<IPerformanceReportService, PerformanceReportService>();
            this._services.AddScoped<IHomeService, HomeService>();

            // Repositories
            this._services.AddScoped<IAccountRepository, AccountRepository>();
            this._services.AddScoped<IUserRepository, UserRepository>();
            this._services.AddScoped<ITicketRepository, TicketRepository>();
            this._services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            this._services.AddScoped<ITeamRepository, TeamRepository>();
            this._services.AddScoped<IAdminRepository, AdminRepository>();
            this._services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
            this._services.AddScoped<INotificationRepository, NotificationRepository>();
            this._services.AddScoped<IPerformanceReportRepository, PerformanceReportRepository>();
            this._services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
            this._services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

            // Manager Class
            this._services.AddScoped<SignInManager>();

            this._services.AddHttpClient();
        }
    }
}
