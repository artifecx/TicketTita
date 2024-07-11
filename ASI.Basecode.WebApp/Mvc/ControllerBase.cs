﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Build.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Services.Exceptions;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;

namespace ASI.Basecode.WebApp.Mvc
{
    /// <summary>
    /// Declare ControllerBase.
    /// </summary>
    public class ControllerBase<TController> : Controller where TController : class
    {
        /// <summary>AppConfiguration</summary>
        protected readonly IConfiguration _configuration;

        /// <summary>HttpContextAccessor</summary>
        protected readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>Logger</summary>
        protected ILogger _logger;

        /// <summary>Session</summary>
        protected ISession _session => _httpContextAccessor.HttpContext.Session;

        /// <summary>
        /// Initializes a new instance of the ControllerBase{TController} class.
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="localizer">Localizer</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="mapper">Mapper</param>
        public ControllerBase(
                                IHttpContextAccessor httpContextAccessor,
                                ILoggerFactory loggerFactory,
                                IConfiguration configuration,
                                IMapper mapper = null)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._logger = loggerFactory.CreateLogger<TController>();
            this._configuration = configuration;
            this._mapper = mapper;
        }

        /// <summary>Mapper</summary>
        protected IMapper _mapper { get; set; }

        /// <summary>
        /// Get Email.
        /// </summary>
        public string UserId
        {
            get { return User.FindFirst(ClaimTypes.NameIdentifier).Value; }
        }

        /// <summary>
        /// Get UserName.
        /// </summary>
        public string UserName
        {
            get { return User.Identity.Name; }
        }

        /// <summary>
        /// Get RoleId.
        /// </summary>
        public string Supervisor
        {
            get { return User.FindFirst(ClaimTypes.Role).Value; }
        }

        /// <summary>
        /// Get ClientId.
        /// </summary>
        public string ClientId
        {
            get { return User.FindFirst("ClientId").Value; }
        }

        /// <summary>
        /// Get ClientSystemId
        /// </summary>
        public string ClientSystemId
        {
            get { return User.FindFirst("ClientSystemId").Value; }
        }

        /// <summary>
        /// Get ClientSystemName
        /// </summary>
        public string ClientSystemName
        {
            get { return User.FindFirst("ClientSystemName").Value; }
        }

        /// <summary>
        /// Get ClientUserRole.
        /// </summary>
        public string ClientUserRole
        {
            get { return User.FindFirst("ClientUserRole").Value; }
        }

        /// <summary>
        /// Return filter default if expiration session.
        /// </summary>
        /// <param name="context">context</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <summary>
        /// OnActionExecuted.
        /// </summary>
        /// <param name="context">context</param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <summary>
        /// Write Log on Exception 
        /// </summary>
        protected void HandleExceptionLog(Exception ex, string request)
        {
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            string actionMethod = this.ControllerContext.RouteData.Values["action"].ToString();

            StringBuilder logContent = new StringBuilder();
            logContent.AppendLine($"\n======================================== start ========================================");
            logContent.AppendLine($"■ API Controller Name: \n\t{controllerName}");
            logContent.AppendLine($"■ API Action Method: \n\t{actionMethod}");
            logContent.AppendLine($"■ API Request Model: \n\t{request}");
            logContent.AppendLine($"■ Exception Message: \n\t{ex.Message}");
            logContent.AppendLine($"■ Exception StackTrace: \n\t{ex.StackTrace}");
            logContent.AppendLine($"========================================= end =========================================\r\n");

            this._logger.LogError(logContent.ToString());
        }

        /// <summary>
        /// Starts the log.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public void StartLog(string methodName)
        {
            _logger.LogInformation($"=======Ticket : {methodName} Started=======");
        }

        /// <summary>
        /// Ends the log.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public void EndLog(string methodName)
        {
            _logger.LogInformation($"=======Ticket : {methodName} Ended=======");
        }

        #region Exception Handlers
        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="actionName">Name of the action.</param>
        public IActionResult HandleException(Func<IActionResult> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return View("Error");
            }
            finally
            {
                EndLog(actionName);
            }
        }
        public JsonResult HandleException(Func<JsonResult> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return new JsonResult(new { success = false, error = "An error occurred. Please try again later." });
            }
            finally
            {
                EndLog(actionName);
            }
        }

        public FileResult HandleException(Func<FileResult> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return null;
            }
            finally
            {
                EndLog(actionName);
            }
        }
        #endregion Exception Handlers

        #region Async Exception Handlers
        public async Task<IActionResult> HandleExceptionAsync(Func<Task<IActionResult>> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return await action();
            }
            catch (DuplicateTicketException ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                _logger.LogError(ex, $"Error in {actionName}");
                return RedirectToAction(actionName);
            }
            catch (NoChangesException ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                _logger.LogError(ex, $"Error in {actionName}");
                return RedirectToAction(actionName, new { id = ex.Id });
            }
            catch (InvalidFileException ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
                _logger.LogError(ex, $"Error in {actionName}");
                return actionName == "Create" ? RedirectToAction(actionName) : RedirectToAction(actionName, new { id = ex.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return View("Error");
            }
            finally
            {
                EndLog(actionName);
            }
        }

        public async Task<JsonResult> HandleExceptionAsync(Func<Task<JsonResult>> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return new JsonResult(new { success = false, error = "An error occurred. Please try again later." });
            }
            finally
            {
                EndLog(actionName);
            }
        }

        public async Task<FileResult> HandleExceptionAsync(Func<Task<FileResult>> action, string actionName)
        {
            try
            {
                StartLog(actionName);
                return await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {actionName}");
                return null;
            }
            finally
            {
                EndLog(actionName);
            }
        }

        #endregion Async Exception Handlers
    }
}
