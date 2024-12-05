using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Task : BaseTaskDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public override string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public override string? Description { get; set; }        

        public DateTime CreatedAt { get; set; }
    }
}