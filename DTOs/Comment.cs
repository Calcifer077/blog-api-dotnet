using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs;

public class CreateCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = "";

    [Required]
    public int PostId { get; set; }
}

public class UpdateCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = "";
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string AuthorUsername { get; set; } = null!;
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; }
}
