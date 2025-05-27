using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace TaskManagerApi.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<Models.Task>> GetAllTasksAsync();
        Task<Models.Task?> GetTaskByIdAsync(int id);
        Task<Models.Task> CreateTaskAsync(CreateTaskDto taskDto);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
    }
}
