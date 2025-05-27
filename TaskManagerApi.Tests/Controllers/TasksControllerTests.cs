using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagerApi.Controllers;
using TaskManagerApi.Services;
using Xunit;
using TodoTask = Models.Task; // Alias for Models.Task

namespace TaskManagerApi.Tests.Controllers
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<ILogger<TasksController>> _loggerMock;
        private readonly TasksController _tasksController;

        public TasksControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _loggerMock = new Mock<ILogger<TasksController>>();
            _tasksController = new TasksController(_taskServiceMock.Object, _loggerMock.Object);
        }

        // --- GetTasks Tests ---
        [Fact]
        public async Task GetTasks_WhenServiceReturnsTasks_ShouldReturnOkObjectResultWithTasks()
        {
            // Arrange
            var tasks = new List<TodoTask>
            {
                new TodoTask { Id = 1, Title = "Task 1" },
                new TodoTask { Id = 2, Title = "Task 2" }
            };
            _taskServiceMock.Setup(s => s.GetAllTasksAsync()).ReturnsAsync(tasks);

            // Act
            var result = await _tasksController.GetTasks();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TodoTask>>(actionResult.Value);
            returnedTasks.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTasks_WhenServiceReturnsEmptyList_ShouldReturnOkObjectResultWithEmptyList()
        {
            // Arrange
            _taskServiceMock.Setup(s => s.GetAllTasksAsync()).ReturnsAsync(new List<TodoTask>());

            // Act
            var result = await _tasksController.GetTasks();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TodoTask>>(actionResult.Value);
            returnedTasks.Should().BeEmpty();
        }

        // --- GetTask(id) Tests ---
        [Fact]
        public async Task GetTaskById_WhenTaskExists_ShouldReturnOkObjectResultWithTask()
        {
            // Arrange
            var task = new TodoTask { Id = 1, Title = "Test Task" };
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(task);

            // Act
            var result = await _tasksController.GetTask(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTask = Assert.IsType<TodoTask>(actionResult.Value);
            returnedTask.Id.Should().Be(1);
            returnedTask.Title.Should().Be("Test Task");
        }

        [Fact]
        public async Task GetTaskById_WhenTaskDoesNotExist_ShouldReturnNotFoundResult()
        {
            // Arrange
            _taskServiceMock.Setup(s => s.GetTaskByIdAsync(99)).ReturnsAsync((TodoTask)null);

            // Act
            var result = await _tasksController.GetTask(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // --- CreateTask(dto) Tests ---
        [Fact]
        public async Task CreateTask_WhenServiceCreatesTask_ShouldReturnCreatedAtActionResultWithTask()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "Desc" };
            var createdTask = new TodoTask { Id = 1, Title = "New Task", Description = "Desc", CreatedAt = System.DateTime.UtcNow };
            _taskServiceMock.Setup(s => s.CreateTaskAsync(createTaskDto)).ReturnsAsync(createdTask);

            // Act
            var result = await _tasksController.CreateTask(createTaskDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedTask = Assert.IsType<TodoTask>(actionResult.Value);
            actionResult.ActionName.Should().Be(nameof(TasksController.GetTask));
            actionResult.RouteValues["id"].Should().Be(1);
            returnedTask.Title.Should().Be("New Task");
        }

        // --- UpdateTask(id, dto) Tests ---
        [Fact]
        public async Task UpdateTask_WhenServiceUpdatesTask_ShouldReturnNoContentResult()
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Task", Description = "Updated Desc" };
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(1, updateTaskDto)).ReturnsAsync(true);

            // Act
            var result = await _tasksController.UpdateTask(1, updateTaskDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateTask_WhenServiceCannotUpdateTask_ShouldReturnNotFoundResult()
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto { Title = "Non Existent", Description = "Task" };
            _taskServiceMock.Setup(s => s.UpdateTaskAsync(99, updateTaskDto)).ReturnsAsync(false);

            // Act
            var result = await _tasksController.UpdateTask(99, updateTaskDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // --- DeleteTask(id) Tests ---
        [Fact]
        public async Task DeleteTask_WhenServiceDeletesTask_ShouldReturnNoContentResult()
        {
            // Arrange
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _tasksController.DeleteTask(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_WhenServiceCannotDeleteTask_ShouldReturnNotFoundResult()
        {
            // Arrange
            _taskServiceMock.Setup(s => s.DeleteTaskAsync(99)).ReturnsAsync(false);

            // Act
            var result = await _tasksController.DeleteTask(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
