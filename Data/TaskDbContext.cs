using Microsoft.EntityFrameworkCore;
using TodoTask = Models.Task;

public class TaskDbContext : DbContext
{
  public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
  {
  }

  public required DbSet<TodoTask> Tasks { get; set; }
}