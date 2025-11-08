using Microsoft.AspNetCore.Mvc;
using PostManage.Models.DTOs;
using PostManage.Services;

namespace PostManage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// Get all posts with optional search and sort
    /// </summary>
    /// <param name="search">Search posts by name</param>
    /// <param name="sortOrder">Sort order: 'asc' or 'A-Z' for ascending, 'desc' or 'Z-A' for descending</param>
    /// <returns>List of posts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PostResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts(
        [FromQuery] string? search,
        [FromQuery] string? sortOrder)
    {
        try
        {
            var posts = await _postService.GetAllPostsAsync(search, sortOrder);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts");
            return StatusCode(500, new { message = "An error occurred while retrieving posts" });
        }
    }

    /// <summary>
    /// Get a post by ID
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <returns>Post details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostResponseDto>> GetPost(int id)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound(new { message = $"Post with ID {id} not found" });
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post with ID {PostId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the post" });
        }
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    /// <param name="dto">Post data</param>
    /// <returns>Created post</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _postService.CreatePostAsync(dto);
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { message = "An error occurred while creating the post" });
        }
    }

    /// <summary>
    /// Update an existing post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <param name="dto">Updated post data</param>
    /// <returns>Updated post</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PostResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostResponseDto>> UpdatePost(int id, [FromBody] UpdatePostDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _postService.UpdatePostAsync(id, dto);

            if (post == null)
            {
                return NotFound(new { message = $"Post with ID {id} not found" });
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post with ID {PostId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the post" });
        }
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    /// <param name="id">Post ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            var deleted = await _postService.DeletePostAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = $"Post with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post with ID {PostId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the post" });
        }
    }
}

