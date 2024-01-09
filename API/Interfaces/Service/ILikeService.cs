using API.DTOs;
using API.Helpers;

namespace API.Interfaces.Service;

public interface ILikeService
{
    Task AddLike(string username, int sourceUserId);
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
}
