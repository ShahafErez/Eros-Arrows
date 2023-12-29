using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;

    public LikesRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if (predicate == "liked")
        {
            likes = likes.Where(like => like.SourceUserId == userId);
            users = likes.Select(like => like.TargetUser);
        }
        if (predicate == "likedBy")
        {
            likes = likes.Where(like => like.TargetUserId == userId);
            users = likes.Select(like => like.SourceUser);
        }

        return await users.Select(user => new LikeDto
        {
            Id = user.Id,
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
            City = user.City
        }).ToListAsync();
    }

    public async Task<User> GetUserWithLikes(int userId)
    {
        return await _context.Users
        .Include(x => x.LikedUsers)
        .FirstOrDefaultAsync(x => x.Id == userId);
    }
}
