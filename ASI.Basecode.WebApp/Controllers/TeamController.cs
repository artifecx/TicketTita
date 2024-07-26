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
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for handling team-related operations.
    /// </summary>
    [Route("team")]
    public class TeamController : ControllerBase<TeamController>
    {
        private readonly ITeamService _teamService;
        private readonly ITicketService _ticketService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamController"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="teamService">The team service.</param>
        /// <param name="ticketService">The ticket service.</param>
        /// <param name="userPreferences">The user preferences service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public TeamController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ITeamService teamService,
            ITicketService ticketService,
            IUserPreferencesService userPreferences,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper, userPreferences)
        {
            this._teamService = teamService;
            this._ticketService = ticketService;
        }

        #region GET Methods 
        /// <summary>
        /// Show all teams.
        /// </summary>
        /// <param name="sortBy">The sort by option.</param>
        /// <param name="filterBy">The filter by option.</param>
        /// <param name="specialization">The specialization filter.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [Authorize(Policy = "Admin")]
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAll(string sortBy, string filterBy, string specialization, int pageIndex = 1)
        {
            return await HandleExceptionAsync(async () =>
            {
                var teams = await _teamService.GetAllAsync(sortBy, filterBy, specialization, pageIndex, 5);

                ViewData["FilterBy"] = filterBy;
                ViewData["SortBy"] = sortBy;
                ViewData["Specialization"] = specialization;
                ViewBag.CTs = (await _ticketService.GetCategoryTypesAsync())
                                .Where(ct => !ct.CategoryName.Contains("Other"))
                                .OrderBy(ct => ct.CategoryName).ToList();

                return View("ViewAll", teams);
            }, "GetAll");
        }

        /// <summary>
        /// Views the selected team.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="showModal">The show modal.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpGet]
        [Authorize(Policy = "Admin")]
        [Route("view")]
        public async Task<IActionResult> ViewTeam(string id, string showModal = null)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = Errors.InvalidTeamId;
                    return RedirectToAction("GetAll");
                }

                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    TempData["ErrorMessage"] = Errors.TeamNotFound;
                    return RedirectToAction("GetAll");
                }
                var agents = await _teamService.GetAgentsAsync();
                var teams = await _teamService.GetAllStrippedAsync();

                ViewBag.ShowModal = showModal;
                ViewBag.AssignedAgents = agents.Where(x => x.TeamMember?.TeamId == id).ToList();
                ViewBag.UnassignedAgents = agents.Where(x => x.TeamMember == null).ToList();
                ViewBag.Teams = teams.Where(x => x.TeamId != id).ToList();
                ViewBag.CTs = (await _ticketService.GetCategoryTypesAsync())
                                .Where(ct => !ct.CategoryName.Contains("Other"))
                                .OrderBy(ct => ct.CategoryName).ToList();

                return View("ViewTeam", team);
            }, "ViewTeam");
        }
        #endregion GET Methods

        #region POST Methods        
        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="model">The team view model.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [Route("create")]
        public async Task<IActionResult> Create(TeamViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.AddAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessCreateTeam;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorCreateTeam;
                return Json(new { success = false });
            }, "Create");
        }

        /// <summary>
        /// Updates the selected team.
        /// </summary>
        /// <param name="model">The team view model.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [Route("update")]
        public async Task<IActionResult> Update(TeamViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.UpdateAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessUpdateTeam;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorUpdateTeam;
                return Json(new { success = false });
            }, "Update");
        }

        /// <summary>
        /// Deletes the selected team.
        /// </summary>
        /// <param name="id">The team identifier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [Route("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.DeleteAsync(id);
                    TempData["SuccessMessage"] = Common.SuccessDeleteTeam;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorDeleteTeam;
                return Json(new { success = false });
            }, "Delete");
        }

        /// <summary>
        /// Assigns an agent to a team.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [Route("assignagent")]
        public async Task<IActionResult> AssignAgent(string teamId, string agentId)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.AddTeamMemberAsync(teamId, agentId);
                    TempData["SuccessMessage"] = Common.SuccessAssignAgent;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorAssignAgent;
                return Json(new { success = false });
            }, "AssignAgent");
        }

        /// <summary>
        /// Reassigns an agent to a different team.
        /// </summary>
        /// <param name="oldTeamId">The old team identifier.</param>
        /// <param name="newTeamId">The new team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>.</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [Route("reassignagent")]
        public async Task<IActionResult> ReassignAgent(string oldTeamId, string newTeamId, string agentId)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.RemoveTeamMemberAsync(oldTeamId, agentId);
                    if (!string.IsNullOrEmpty(newTeamId))
                    {
                        await _teamService.AddTeamMemberAsync(newTeamId, agentId);
                    }

                    TempData["SuccessMessage"] = string.IsNullOrEmpty(newTeamId) ?
                        Common.SuccessUnassignAgent : Common.SuccessReassignAgent;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorReassignAgent;
                return Json(new { success = false });
            }, "ReassignAgent");
        }
        #endregion POST Methods
    }
}
