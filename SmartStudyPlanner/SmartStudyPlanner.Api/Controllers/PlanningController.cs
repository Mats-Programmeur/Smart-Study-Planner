using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Extensions;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("planning")]
    [Authorize(Roles = RoleNames.Student)]
    public class PlanningController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly DeadlineService _deadlineService;

        public PlanningController(TaskService taskService, DeadlineService deadlineService)
        {
            _taskService = taskService;
            _deadlineService = deadlineService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.GetUserId();
            var tasks = await _taskService.GetAllForUserAsync(userId);
            var deadlines = await _deadlineService.GetAllForUserAsync(userId);

            return Ok(new PlanningOverviewResponse(tasks, deadlines));
        }
    }
}
