using PostManage.Models.DTOs;

namespace PostManage.Services;

public interface IPostService
{
    Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(string? search, string? sortOrder);
    Task<PostResponseDto?> GetPostByIdAsync(int id);
    Task<PostResponseDto> CreatePostAsync(CreatePostDto dto);
    Task<PostResponseDto?> UpdatePostAsync(int id, UpdatePostDto dto);
    Task<bool> DeletePostAsync(int id);
}

