using AutoMapper; // Add this using
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;

namespace TaskManagerApi.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskDbContext _context;
        private readonly ILogger<TaskService> _logger;
        private readonly IMapper _mapper; // Add IMapper

        public TaskService(TaskDbContext context, ILogger<TaskService> logger, IMapper mapper) // Inject IMapper
        {
            _context = context;
            _logger = logger;
            _mapper = mapper; // Assign IMapper
        }

        public async Task<IEnumerable<Models.Task>> GetAllTasksAsync()
        {
            _logger.LogInformation("Fetching all tasks from service");
            var tasks = await _context.Tasks.ToListAsync();
            _logger.LogInformation("Retrieved {TaskCount} tasks from service", tasks.Count);
            return tasks;
        }

        public async Task<Models.Task?> GetTaskByIdAsync(int id)
        {
            _logger.LogInformation("Fetching task with ID {TaskId} from service", id);
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found in service", id);
            }
            else
            {
                _logger.LogInformation("Task with ID {TaskId} retrieved successfully from service", id);
            }
            return task;
        }

        public async Task<Models.Task> CreateTaskAsync(CreateTaskDto taskDto)
        {
            _logger.LogInformation("Attempting to create a new task with Title: {TaskTitle} in service", taskDto.Title);
            var newTask = _mapper.Map<Models.Task>(taskDto); // Use AutoMapper
            newTask.CreatedAt = System.DateTime.UtcNow; // Set CreatedAt separately

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task created successfully with ID {TaskId} in service", newTask.Id);
            return newTask;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for update.", id);
                return false;
            }

            _mapper.Map(taskDto, task); // Apply changes from DTO to entity

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Task with ID {TaskId} updated successfully.", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency exception while updating Task with ID {TaskId}.", id);
                // Optionally, re-check if the task still exists to differentiate between
                // true concurrency (updated by another process) vs. deleted by another process.
                var taskStillExists = await _context.Tasks.AnyAsync(e => e.Id == id);
                if (!taskStillExists)
                {
                    _logger.LogWarning("Task with ID {TaskId} was deleted by another process during update.", id);
                    return false; // Or throw a specific "deleted" exception
                }
                throw; // Re-throw for global handler to catch as a server-side issue
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            _logger.LogInformation("Attempting to delete task with ID {TaskId} in service", id);
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion in service", id);
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Task with ID {TaskId} deleted successfully in service", id);
            return true;
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.Tasks.AnyAsync(e => e.Id == id);
        }
    }
}
