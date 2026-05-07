using System.Security.Claims;
using AutoMapper;
using BlogApi.Data;
using BlogApi.DTOs;
using BlogApi.Mappings;
using BlogApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers;

// ActionResult<T> → Has a response body
// IActionResult → No response body

[ApiController]
[Route("api/[controller]")] // api/posts
public class PostsController : ControllerBase
{
    // ControllerBase -> no views, only JSON responses

    // Lets us access the database
    private readonly AppDbContext _db;

    // converts between entity models and DTOs
    private readonly IMapper _mapper;

    public PostsController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // Get api/posts
    [HttpGet]
    public async Task<ActionResult<List<PostResponseDto>>> GetAll()
    {
        // Fetch all post and include the user and comments, than sort by 'CreatedAt' in ascending order.
        var posts = await _db
            .Posts.Include(p => p.User)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Convert 'posts' to List<dto> ('PostResponseDto')
        return Ok(_mapper.Map<List<PostResponseDto>>(posts));
    }

    // Get api/posts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetById(int id)
    {
        // Find first post with given ID.
        var post = await _db
            .Posts.Include(p => p.User)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Returns 404 (not found)
        if (post == null)
            return NotFound();

        // Converts 'post' to dto ('PostResponseDto')
        return Ok(_mapper.Map<PostResponseDto>(post));
    }

    // Post api/posts [Authorize]
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostResponseDto>> Create(CreatePostDto dto)
    {
        // Extract logged-in user ID from JWT token
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // converts incoming dto to post
        var post = _mapper.Map<Post>(dto);
        // assigns current user as owner
        post.UserId = userId;

        // adds post to database
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // explicitly loads related user data
        await _db.Entry(post).Reference(p => p.User).LoadAsync();

        // returns 201 (created)
        // includes location of new resource
        // created post data as 'PostResponseDto'
        return CreatedAtAction(
            nameof(GetById),
            new { id = post.Id },
            _mapper.Map<PostResponseDto>(post)
        );
    }

    // Put api/posts/5 [Authorize]
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdatePostDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var post = await _db.Posts.FindAsync(id);

        if (post == null)
            return NotFound();

        // Only the user that created this post can modify this, else forbid 403
        if (post.UserId != userId)
            return Forbid();

        if (dto.Title != null)
            post.Title = dto.Title;
        if (dto.Content != null)
            post.Content = dto.Content;

        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Delete api/posts/5 [Authorize]
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var post = await _db.Posts.FindAsync(id);
        if (post == null)
            return NotFound();
        if (post.UserId != userId)
            return Forbid();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
