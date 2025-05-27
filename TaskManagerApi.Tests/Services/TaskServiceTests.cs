using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks; // This will now correctly be System.Threading.Tasks.Task
using TaskManagerApi.Services;
using Models;
using Microsoft.EntityFrameworkCore;
// End of essential using directives. Original usings (if any non-essential) will follow.
using System; // Preserved
using System.Linq; // Preserved
using TaskManagerApi.Mappings; // Preserved
using ApiTask = Models.Task; // MODIFIED ALIAS: Changed from "Task" to "ApiTask"

namespace TaskManagerApi.Tests.Services
{
    public class TaskServiceTests : IDisposable
    {
        private readonly TaskDbContext _context;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test run
                .Options;
            _context = new TaskDbContext(options);

            // Setup AutoMapper
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new TaskProfile());
            });
            _mapper = mappingConfig.CreateMapper();

            // Setup Logger Mock
            _loggerMock = new Mock<ILogger<TaskService>>();

            // Initialize Service
            _taskService = new TaskService(_context, _loggerMock.Object, _mapper);
        }

        // All usages of "Task" that meant "Models.Task" are now "ApiTask"
        private async Task SeedDataAsync(IEnumerable<ApiTask> tasks) 
        {
            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();
        }

        // --- GetAllTasksAsync Tests ---
        [Fact]
        public async Task GetAllTasksAsync_WhenNoTasksExist_ShouldReturnEmptyList() // Return type is System.Threading.Tasks.Task
        {
            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllTasksAsync_WhenTasksExist_ShouldReturnListOfTasks() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var tasksToSeed = new List<ApiTask> // Changed to ApiTask
            {
                new ApiTask { Id = 1, Title = "Task 1", Description = "Desc 1", CreatedAt = DateTime.UtcNow },
                new ApiTask { Id = 2, Title = "Task 2", Description = "Desc 2", CreatedAt = DateTime.UtcNow }
            };
            await SeedDataAsync(tasksToSeed);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().HaveCount(2);
            // BeEquivalentTo will compare ApiTask instances with Models.Task from the service, which should work.
            result.Should().BeEquivalentTo(tasksToSeed, options => options.Excluding(t => t.Id)); 
        }

        // --- GetTaskByIdAsync Tests ---
        [Fact]
        public async Task GetTaskByIdAsync_WhenIdExists_ShouldReturnTask() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var taskToSeed = new ApiTask { Id = 1, Title = "Task 1", Description = "Desc 1", CreatedAt = DateTime.UtcNow }; // Changed to ApiTask
            await SeedDataAsync(new List<ApiTask> { taskToSeed }); // Changed to ApiTask

            // Act
            var result = await _taskService.GetTaskByIdAsync(1); // Service returns Models.Task, which is compatible with ApiTask for assertions

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Task 1");
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenIdDoesNotExist_ShouldReturnNull() // Return type is System.Threading.Tasks.Task
        {
            // Act
            var result = await _taskService.GetTaskByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        // --- CreateTaskAsync Tests ---
        [Fact]
        public async Task CreateTaskAsync_ShouldCreateAndReturnTask() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "New Desc" };

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto); // Service returns Models.Task

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0); 
            result.Title.Should().Be("New Task");
            result.Description.Should().Be("New Desc");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var taskInDb = await _context.Tasks.FindAsync(result.Id); // _context.Tasks is DbSet<Models.Task>
            taskInDb.Should().NotBeNull();
            taskInDb.Title.Should().Be("New Task");
        }

        // --- UpdateTaskAsync Tests ---
        [Fact]
        public async Task UpdateTaskAsync_WhenTaskExists_ShouldUpdateAndReturnTrue() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var taskToSeed = new ApiTask { Id = 1, Title = "Old Title", Description = "Old Desc", CreatedAt = DateTime.UtcNow }; // Changed to ApiTask
            await SeedDataAsync(new List<ApiTask> { taskToSeed }); // Changed to ApiTask
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Title", Description = "Updated Desc" };

            // Act
            var result = await _taskService.UpdateTaskAsync(1, updateTaskDto);

            // Assert
            result.Should().BeTrue();
            var updatedTask = await _context.Tasks.FindAsync(1); // _context.Tasks is DbSet<Models.Task>
            updatedTask.Should().NotBeNull();
            updatedTask.Title.Should().Be("Updated Title");
            updatedTask.Description.Should().Be("Updated Desc");
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenTaskDoesNotExist_ShouldReturnFalse() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto { Title = "Non Existent", Description = "Task" };

            // Act
            var result = await _taskService.UpdateTaskAsync(99, updateTaskDto);

            // Assert
            result.Should().BeFalse();
        }
        
        // --- DeleteTaskAsync Tests ---
        [Fact]
        public async Task DeleteTaskAsync_WhenTaskExists_ShouldDeleteAndReturnTrue() // Return type is System.Threading.Tasks.Task
        {
            // Arrange
            var taskToSeed = new ApiTask { Id = 1, Title = "To Delete", Description = "Delete Desc", CreatedAt = DateTime.UtcNow }; // Changed to ApiTask
            await SeedDataAsync(new List<ApiTask> { taskToSeed }); // Changed to ApiTask

            // Act
            var result = await _taskService.DeleteTaskAsync(1);

            // Assert
            result.Should().BeTrue();
            var deletedTask = await _context.Tasks.FindAsync(1); // _context.Tasks is DbSet<Models.Task>
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ShouldReturnFalse() // Return type is System.Threading.Tasks.Task
        {
            // Act
            var result = await _taskService.DeleteTaskAsync(99);

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted(); 
            _context.Dispose();
        }
    }
}
