using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Contracts;
using SmartStudyPlanner.Api.Extensions;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;
using System.Net;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("deadlines")]
    [Authorize(Roles = RoleNames.Student)]
    public class DeadlinesController : ControllerBase
    {
        private readonly DeadlineService _deadlineService;

        public DeadlinesController(DeadlineService deadlineService)
        {
            _deadlineService = deadlineService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _deadlineService.GetAllForUserAsync(User.GetUserId()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveDeadlineRequest request)
        {
            var validationError = Validate(request);

            if (validationError is not null)
            {
                return BadRequest(validationError);
            }

            var deadline = new DeadlineItem
            {
                Titel = WebUtility.HtmlEncode(request.Titel.Trim()),
                Beschrijving = string.IsNullOrWhiteSpace(request.Beschrijving)
                    ? null
                    : WebUtility.HtmlEncode(request.Beschrijving.Trim()),
                Datum = request.Datum,
                EindTijd = request.EindTijd,
                Prioriteit = request.Prioriteit,
                UserId = User.GetUserId()
            };

            return Ok(await _deadlineService.AddAsync(deadline));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SaveDeadlineRequest request)
        {
            var validationError = Validate(request);

            if (validationError is not null)
            {
                return BadRequest(validationError);
            }

            var deadline = await _deadlineService.GetByIdForUserAsync(id, User.GetUserId());

            if (deadline is null)
            {
                return NotFound("Deadline niet gevonden.");
            }

            deadline.Titel = WebUtility.HtmlEncode(request.Titel.Trim());
            deadline.Beschrijving = string.IsNullOrWhiteSpace(request.Beschrijving)
                ? null
                : WebUtility.HtmlEncode(request.Beschrijving.Trim());
            deadline.Datum = request.Datum;
            deadline.EindTijd = request.EindTijd;
            deadline.Prioriteit = request.Prioriteit;

            return Ok(await _deadlineService.UpdateAsync(deadline));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deadline = await _deadlineService.GetByIdForUserAsync(id, User.GetUserId());

            if (deadline is null)
            {
                return NotFound("Deadline niet gevonden.");
            }

            await _deadlineService.DeleteAsync(deadline);
            return NoContent();
        }

        private static string? Validate(SaveDeadlineRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titel))
            {
                return "Titel is verplicht.";
            }

            if (request.Titel.Trim().Length > 120)
            {
                return "Titel mag maximaal 120 tekens bevatten.";
            }

            if (!string.IsNullOrWhiteSpace(request.Beschrijving) && request.Beschrijving.Trim().Length > 500)
            {
                return "Beschrijving mag maximaal 500 tekens bevatten.";
            }

            if (request.Datum == default)
            {
                return "Datum is verplicht.";
            }

            if (request.EindTijd == default)
            {
                return "Eindtijd is verplicht.";
            }

            if (!TaskPriority.All.Contains(request.Prioriteit))
            {
                return "Prioriteit moet Laag, Normaal of Hoog zijn.";
            }

            return null;
        }
    }
}
