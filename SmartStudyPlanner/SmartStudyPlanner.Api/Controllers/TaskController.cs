using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;
using System.Net;

namespace SmartStudyPlanner.Api.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public IActionResult Create(TaskItem task)
        {
            var normalizedTitle = task.Titel?.Trim();
            var normalizedDescription = task.Beschrijving?.Trim();
            var earliestAllowedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                return BadRequest("Titel is verplicht.");
            }

            if (normalizedTitle.Length > 120)
            {
                return BadRequest("Titel mag maximaal 120 tekens bevatten.");
            }

            if (!string.IsNullOrEmpty(normalizedDescription) && normalizedDescription.Length > 500)
            {
                return BadRequest("Beschrijving mag maximaal 500 tekens bevatten.");
            }

            if (task.Datum == default)
            {
                return BadRequest("Datum is verplicht.");
            }

            if (task.Datum < earliestAllowedDate)
            {
                return BadRequest("Je kunt een taak niet verder dan 1 dag terug plannen.");
            }

            if (task.StartTijd == default || task.EindTijd == default)
            {
                return BadRequest("Starttijd en eindtijd zijn verplicht.");
            }

            if (task.EindTijd <= task.StartTijd)
            {
                return BadRequest("Eindtijd moet later zijn dan starttijd.");
            }

            task.Titel = WebUtility.HtmlEncode(normalizedTitle);
            task.Beschrijving = string.IsNullOrWhiteSpace(normalizedDescription)
                ? null
                : WebUtility.HtmlEncode(normalizedDescription);

            var createdTask = _taskService.Add(task);

            return Ok(createdTask);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_taskService.GetAll());
        }
    }
}
