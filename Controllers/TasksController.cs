using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoTask = Models.Task;
using Models;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
  private readonly TaskDbContext _context;

  public TasksController(TaskDbContext context)
  {
    _context = context;
  }

  // Tüm görevleri getir
  [HttpGet]
  public async Task<ActionResult<IEnumerable<TodoTask>>> GetTasks()
  {
    return await _context.Tasks.ToListAsync();
  }

  // ID'ye göre görev getir
  [HttpGet("{id}")]
  public async Task<ActionResult<TodoTask>> GetTask(int id)
  {
    var task = await _context.Tasks.FindAsync(id);

    if (task == null)
    {
      return NotFound();
    }

    return task;
  }

  // Yeni görev oluştur
  [HttpPost]
  public async Task<ActionResult<TodoTask>> CreateTask(CreateTaskDto task)
  {
    var newTask = new TodoTask
    {
      Title = task.Title,
      Description = task.Description,
      CreatedAt = DateTime.UtcNow
    };

    _context.Tasks.Add(newTask);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetTask), new { id = newTask.Id }, newTask);
  }

  // Görevi güncelle
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateDto)
  {
    var task = await _context.Tasks.FindAsync(id);
    
    if (task == null)
    {
        return NotFound();
    }

    task.Title = updateDto.Title;
    task.Description = updateDto.Description;
    
    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!TaskExists(id))
        {
            return NotFound();
        }
        throw;
    }

    return NoContent();
  }

  // Görevi sil
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteTask(int id)
  {
    var task = await _context.Tasks.FindAsync(id);

    if (task == null)
    {
      return NotFound();
    }

    _context.Tasks.Remove(task);
    await _context.SaveChangesAsync();

    return NoContent();
  }

  private bool TaskExists(int id)
  {
    return _context.Tasks.Any(e => e.Id == id);
  }
}