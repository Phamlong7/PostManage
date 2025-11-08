using Microsoft.EntityFrameworkCore;
using PostManage.Data;
using PostManage.Models;
using PostManage.Models.DTOs;

namespace PostManage.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(string? search, string? sortOrder)
    {
        var query = _context.Posts.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        // Apply sorting
        sortOrder = sortOrder?.ToLower();
        if (sortOrder == "desc" || sortOrder == "z-a")
        {
            query = query.OrderByDescending(p => p.Name);
        }
        else // Default: A-Z or asc
        {
            query = query.OrderBy(p => p.Name);
        }

        var posts = await query.ToListAsync();

        return posts.Select(p => new PostResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Image = p.Image,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<PostResponseDto?> GetPostByIdAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return null;
        }

        return new PostResponseDto
        {
            Id = post.Id,
            Name = post.Name,
            Description = post.Description,
            Image = post.Image,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<PostResponseDto> CreatePostAsync(CreatePostDto dto)
    {
        var post = new Post
        {
            Name = dto.Name,
            Description = dto.Description,
            Image = dto.Image,
            CreatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return new PostResponseDto
        {
            Id = post.Id,
            Name = post.Name,
            Description = post.Description,
            Image = post.Image,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<PostResponseDto?> UpdatePostAsync(int id, UpdatePostDto dto)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return null;
        }

        post.Name = dto.Name;
        post.Description = dto.Description;
        post.Image = dto.Image;

        await _context.SaveChangesAsync();

        return new PostResponseDto
        {
            Id = post.Id,
            Name = post.Name,
            Description = post.Description,
            Image = post.Image,
            CreatedAt = post.CreatedAt
        };
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return false;
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return true;
    }
}

