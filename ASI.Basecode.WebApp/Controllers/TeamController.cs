using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class TeamController : ControllerBase<TeamController>
    {
        private readonly ITeamService _teamService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamController"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="teamService">The team service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public TeamController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            ITeamService teamService,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._teamService = teamService;
        }

        /// <summary>Show all teams</summary>
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ViewAll(string sortBy, string filterBy, int pageIndex = 1)
        {
            return await HandleExceptionAsync(async () =>
            {
                var teams = await _teamService.GetAllAsync(sortBy, filterBy, pageIndex, 10);

                ViewData["FilterBy"] = filterBy;
                ViewData["SortBy"] = sortBy;

                return View(teams);
            }, "ViewAll");
        }

        #region GET Methods        
        /// <summary>
        /// Views the selected team.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="showModal">The show modal.</param>
        /// <returns>View</returns>
        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ViewTeam(string id, string showModal = null)
        {
            return await HandleExceptionAsync(async () =>
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                var agents = await _teamService.GetAgentsAsync();
                var teams = await _teamService.GetAllStrippedAsync();

                ViewBag.ShowModal = showModal;
                ViewBag.AssignedAgents = agents.Where(x => x.TeamMember?.TeamId == id).ToList();
                ViewBag.UnassignedAgents = agents.Where(x => x.TeamMember == null).ToList();
                ViewBag.Teams = teams.Where(x => x.TeamId != id).ToList();

                return View(team);
            }, "ViewTeam");
        }
        #endregion GET Methods

        #region POST Methods        
        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create(TeamViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.AddAsync(model);
                    TempData["SuccessMessage"] = "Team created successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while creating the team. Please try again.";
                return Json(new { success = false });
            }, "Create");
        }

        /// <summary>
        /// Edits the selected team.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Edit(TeamViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.UpdateAsync(model);
                    TempData["SuccessMessage"] = "Team updated successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while updating the team. Please try again.";
                return Json(new { success = false });
            }, "Edit");
        }

        /// <summary>
        /// Deletes the selected team.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.DeleteAsync(id);
                    TempData["SuccessMessage"] = "Team deleted successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting the team. Please try again.";
                return Json(new { success = false });
            }, "Delete");
        }

        /// <summary>
        /// Assigns an agent to team.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AssignAgent(string teamId, string agentId)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _teamService.AddTeamMemberAsync(teamId, agentId);
                    TempData["SuccessMessage"] = "Agent assigned successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while assigning the agent. Please try again.";
                return Json(new { success = false });
            }, "AssignAgent");
        }

        /// <summary>
        /// Reassigns an agent to team.
        /// </summary>
        /// <param name="oldTeamId">The old team identifier.</param>
        /// <param name="newTeamId">The new team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>Json success status</returns>
        [HttpPost]
        [Authorize(Policy = "Admin")]
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
                        "Agent unassigned successfully!" : "Agent reassigned successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while reassigning the agent. Please try again.";
                return Json(new { success = false });
            }, "ReassignAgent");
        }
        #endregion POST Methods
    }
}
