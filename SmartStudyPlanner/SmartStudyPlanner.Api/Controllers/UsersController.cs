using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Extensions;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize(Roles = RoleNames.Beheerder)]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserRequest request)
        {
            var result = await _userService.UpdateAsync(id, request, User.GetUserId());

            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }
    }
}
