using System.Security.Claims;
using AutoMapper;
using BlogApi.Data;
using BlogApi.DTOs;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public CommentsController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // '[FromQuery]' means that the 'postId' parameter is optional. Calling 'GET /api/comments' returns all comments across the blog, while 'GET /api/comments?postId = 5' returns only comments for post 5.
    // 'AsQueryable' call is what allows you to conditionally chain '.Where' before hitting the database.
    [HttpGet]
    public async Task<ActionResult<List<CommentResponseDto>>> GetAll([FromQuery] int? postId)
    {
        var query = _db.Comments.Include(c => c.User).AsQueryable();

        // Filter by post if postId provided
        if (postId.HasValue)
            query = query.Where(c => c.PostId == postId.Value);

        var comments = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        return Ok(_mapper.Map<List<CommentResponseDto>>(comments));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentResponseDto>> GetById(int id)
    {
        var comment = await _db.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            return NotFound();

        return Ok(_mapper.Map<CommentResponseDto>(comment));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentResponseDto>> Create(CreateCommentDto dto)
    {
        var postExists = await _db.Posts.AnyAsync(p => p.Id == dto.PostId);

        if (!postExists)
            return NotFound($"Post with id {dto.PostId} not found.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var comment = _mapper.Map<Comment>(dto);
        comment.UserId = userId;

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        // after saving a new comment, the 'User' navigation property is null because EF core only loaded the entity you just created. Calling 'LoadAsync' explicitly fetches the related user so AutoMapper can read 'AuthorUserName; for the 201 response.
        await _db.Entry(comment).Reference(c => c.User).LoadAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = comment.Id },
            _mapper.Map<CommentResponseDto>(comment)
        );
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateCommentDto dto)
    {
        var userId = int.Parse(ClaimTypes.NameIdentifier);

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
            return NotFound();

        if (comment.UserId != userId)
            return Forbid();

        comment.Content = dto.Content;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(ClaimTypes.NameIdentifier);

        var comment = await _db.Comments.FindAsync(id);

        if (comment == null)
            return NotFound();

        if (comment.UserId != userId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
