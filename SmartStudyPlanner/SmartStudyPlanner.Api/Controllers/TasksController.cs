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
    [Route("tasks")]
    [Authorize(Roles = RoleNames.Student)]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _taskService.GetAllForUserAsync(User.GetUserId()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveTaskRequest request)
        {
            var validationError = Validate(request);

            if (validationError is not null)
            {
                return BadRequest(validationError);
            }

            var task = new TaskItem
            {
                Titel = WebUtility.HtmlEncode(request.Titel.Trim()),
                Beschrijving = string.IsNullOrWhiteSpace(request.Beschrijving)
                    ? null
                    : WebUtility.HtmlEncode(request.Beschrijving.Trim()),
                Datum = request.Datum,
                StartTijd = request.StartTijd,
                EindTijd = request.EindTijd,
                Prioriteit = request.Prioriteit,
                GeschatteStudietijdMinuten = CalculateStudyTimeMinutes(request.StartTijd, request.EindTijd),
                Status = request.Status,
                UserId = User.GetUserId()
            };

            return Ok(await _taskService.AddAsync(task));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SaveTaskRequest request)
        {
            var validationError = Validate(request);

            if (validationError is not null)
            {
                return BadRequest(validationError);
            }

            var task = await _taskService.GetByIdForUserAsync(id, User.GetUserId());

            if (task is null)
            {
                return NotFound("Taak niet gevonden.");
            }

            task.Titel = WebUtility.HtmlEncode(request.Titel.Trim());
            task.Beschrijving = string.IsNullOrWhiteSpace(request.Beschrijving)
                ? null
                : WebUtility.HtmlEncode(request.Beschrijving.Trim());
            task.Datum = request.Datum;
            task.StartTijd = request.StartTijd;
            task.EindTijd = request.EindTijd;
            task.Prioriteit = request.Prioriteit;
            task.GeschatteStudietijdMinuten = CalculateStudyTimeMinutes(request.StartTijd, request.EindTijd);
            task.Status = request.Status;

            return Ok(await _taskService.UpdateAsync(task));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetByIdForUserAsync(id, User.GetUserId());

            if (task is null)
            {
                return NotFound("Taak niet gevonden.");
            }

            await _taskService.DeleteAsync(task);
            return NoContent();
        }

        private static string? Validate(SaveTaskRequest request)
        {
            var earliestAllowedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

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

            if (request.Datum == default || request.Datum < earliestAllowedDate)
            {
                return "Je kunt een taak niet verder dan 1 dag terug plannen.";
            }

            if (request.StartTijd == default || request.EindTijd == default || request.EindTijd <= request.StartTijd)
            {
                return "Eindtijd moet later zijn dan starttijd.";
            }

            if (!TaskPriority.All.Contains(request.Prioriteit))
            {
                return "Prioriteit moet Laag, Normaal of Hoog zijn.";
            }

            if (!Models.TaskStatus.All.Contains(request.Status))
            {
                return "Status moet Gepland, Bezig of Afgerond zijn.";
            }

            return null;
        }

        private static int CalculateStudyTimeMinutes(TimeOnly startTijd, TimeOnly eindTijd)
        {
            return (int)(eindTijd.ToTimeSpan() - startTijd.ToTimeSpan()).TotalMinutes;
        }
    }
}
