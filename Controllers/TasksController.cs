using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagerApi.Services;
using Models;
using TodoTask = Models.Task; // Alias for Models.Task to avoid conflict

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    // Tüm görevleri getir
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoTask>>> GetTasks()
    {
        _logger.LogInformation("Controller: Fetching all tasks");
        var tasks = await _taskService.GetAllTasksAsync();
        _logger.LogInformation("Controller: Retrieved {TaskCount} tasks", tasks.Count());
        return Ok(tasks);
    }

    // ID'ye göre görev getir
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoTask>> GetTask(int id)
    {
        _logger.LogInformation("Controller: Fetching task with ID {TaskId}", id);
        var task = await _taskService.GetTaskByIdAsync(id);

        if (task == null)
        {
            _logger.LogWarning("Controller: Task with ID {TaskId} not found", id);
            return NotFound();
        }
        _logger.LogInformation("Controller: Task with ID {TaskId} retrieved successfully", id);
        return Ok(task);
    }

    // Yeni görev oluştur
    [HttpPost]
    public async Task<ActionResult<TodoTask>> CreateTask(CreateTaskDto taskDto)
    {
        _logger.LogInformation("Controller: Attempting to create a new task with Title: {TaskTitle}", taskDto.Title);
        var newTask = await _taskService.CreateTaskAsync(taskDto);
        _logger.LogInformation("Controller: Task created successfully with ID {TaskId}", newTask.Id);
        return CreatedAtAction(nameof(GetTask), new { id = newTask.Id }, newTask);
    }

    // Görevi güncelle
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateDto)
    {
        _logger.LogInformation("Controller: Attempting to update task with ID {TaskId}", id);
        var success = await _taskService.UpdateTaskAsync(id, updateDto);

        if (!success)
        {
            _logger.LogWarning("Controller: Task with ID {TaskId} not found for update or update failed", id);
            return NotFound(); // Or appropriate error if update failed for other reasons
        }
        
        _logger.LogInformation("Controller: Task with ID {TaskId} updated successfully", id);
        return NoContent();
    }

    // Görevi sil
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        _logger.LogInformation("Controller: Attempting to delete task with ID {TaskId}", id);
        var success = await _taskService.DeleteTaskAsync(id);

        if (!success)
        {
            _logger.LogWarning("Controller: Task with ID {TaskId} not found for deletion or delete failed", id);
            return NotFound();
        }

        _logger.LogInformation("Controller: Task with ID {TaskId} deleted successfully", id);
        return NoContent();
    }
}