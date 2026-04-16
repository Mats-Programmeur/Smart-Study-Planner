using Microsoft.AspNetCore.Mvc;
using SmartStudyPlanner.Api.Models;
using SmartStudyPlanner.Api.Services;

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
            if (string.IsNullOrEmpty(task.Titel))
            {
                return BadRequest("Titel is verplicht");
            }

            // 🔐 XSS mitigatie
            task.Titel = System.Net.WebUtility.HtmlEncode(task.Titel);
            task.Beschrijving = System.Net.WebUtility.HtmlEncode(task.Beschrijving);

            _taskService.Add(task);

            return Ok(task);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_taskService.GetAll());
        }
    }
}
