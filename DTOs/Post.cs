using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs;

// DTOs/Posts/CreatePostDto.cs
public class CreatePostDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    public string Content { get; set; } = "";
}

// DTOs/Posts/UpdatePostDto.cs
public class UpdatePostDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    public string? Content { get; set; }
}

// DTOs/Posts/PostResponseDto.cs
public class PostResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string AuthorUsername { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int CommentCount { get; set; }
}
