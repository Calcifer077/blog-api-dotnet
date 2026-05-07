using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Post
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
