using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Extensions;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("advice")]
    [Authorize(Roles = RoleNames.Student)]
    public class AdviceController : ControllerBase
    {
        private readonly AdviceService _adviceService;

        public AdviceController(AdviceService adviceService)
        {
            _adviceService = adviceService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _adviceService.GetAdviceForUserAsync(User.GetUserId()));
        }
    }
}
