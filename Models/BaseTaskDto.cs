namespace Models
{
    public abstract class BaseTaskDto
    {
        public virtual string Title { get; set; } = string.Empty;
        public virtual string? Description { get; set; }
    }
} 