using API.DTOs;
using API.Helpers;

namespace API.Interfaces.Service;

public interface ILikeService
{
    Task<bool> AddLike(string username, int sourceUserId);
    Task<PagedList<LikeDto>> getUserLikes(LikesParams likesParams);
}
