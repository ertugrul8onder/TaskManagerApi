using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagerApi.Mappings;
using TaskManagerApi.Services;
using Xunit;
using Task = Models.Task;

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

        private async Task SeedDataAsync(IEnumerable<Task> tasks)
        {
            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();
        }

        // --- GetAllTasksAsync Tests ---
        [Fact]
        public async Task GetAllTasksAsync_WhenNoTasksExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllTasksAsync_WhenTasksExist_ShouldReturnListOfTasks()
        {
            // Arrange
            var tasksToSeed = new List<Task>
            {
                new Task { Id = 1, Title = "Task 1", Description = "Desc 1", CreatedAt = DateTime.UtcNow },
                new Task { Id = 2, Title = "Task 2", Description = "Desc 2", CreatedAt = DateTime.UtcNow }
            };
            await SeedDataAsync(tasksToSeed);

            // Act
            var result = await _taskService.GetAllTasksAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(tasksToSeed, options => options.Excluding(t => t.Id)); // In-memory DB assigns Ids
        }

        // --- GetTaskByIdAsync Tests ---
        [Fact]
        public async Task GetTaskByIdAsync_WhenIdExists_ShouldReturnTask()
        {
            // Arrange
            var taskToSeed = new Task { Id = 1, Title = "Task 1", Description = "Desc 1", CreatedAt = DateTime.UtcNow };
            await SeedDataAsync(new List<Task> { taskToSeed });

            // Act
            var result = await _taskService.GetTaskByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Task 1");
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _taskService.GetTaskByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        // --- CreateTaskAsync Tests ---
        [Fact]
        public async Task CreateTaskAsync_ShouldCreateAndReturnTask()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto { Title = "New Task", Description = "New Desc" };

            // Act
            var result = await _taskService.CreateTaskAsync(createTaskDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0); // DB should assign an ID
            result.Title.Should().Be("New Task");
            result.Description.Should().Be("New Desc");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var taskInDb = await _context.Tasks.FindAsync(result.Id);
            taskInDb.Should().NotBeNull();
            taskInDb.Title.Should().Be("New Task");
        }

        // --- UpdateTaskAsync Tests ---
        [Fact]
        public async Task UpdateTaskAsync_WhenTaskExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var taskToSeed = new Task { Id = 1, Title = "Old Title", Description = "Old Desc", CreatedAt = DateTime.UtcNow };
            await SeedDataAsync(new List<Task> { taskToSeed });
            var updateTaskDto = new UpdateTaskDto { Title = "Updated Title", Description = "Updated Desc" };

            // Act
            var result = await _taskService.UpdateTaskAsync(1, updateTaskDto);

            // Assert
            result.Should().BeTrue();
            var updatedTask = await _context.Tasks.FindAsync(1);
            updatedTask.Should().NotBeNull();
            updatedTask.Title.Should().Be("Updated Title");
            updatedTask.Description.Should().Be("Updated Desc");
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenTaskDoesNotExist_ShouldReturnFalse()
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
        public async Task DeleteTaskAsync_WhenTaskExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var taskToSeed = new Task { Id = 1, Title = "To Delete", Description = "Delete Desc", CreatedAt = DateTime.UtcNow };
            await SeedDataAsync(new List<Task> { taskToSeed });

            // Act
            var result = await _taskService.DeleteTaskAsync(1);

            // Assert
            result.Should().BeTrue();
            var deletedTask = await _context.Tasks.FindAsync(1);
            deletedTask.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskDoesNotExist_ShouldReturnFalse()
        {
            // Act
            var result = await _taskService.DeleteTaskAsync(99);

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted(); // Clean up in-memory database after each test
            _context.Dispose();
        }
    }
}
