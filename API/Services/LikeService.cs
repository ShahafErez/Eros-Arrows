using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Interfaces.Service;


namespace API.Services;

public class LikeService : ILikeService
{
    private readonly IUnitOfWork _unitOfWork;

    public LikeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddLike(string username, int sourceUserId)
    {
        var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username.ToLower());
        var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

        if (likedUser == null) throw new KeyNotFoundException(string.Format("User {0} was not found", username));
        var likedUserId = likedUser.Id;

        if (sourceUser.UserName == username) throw new BadHttpRequestException("A user cannot like it own profile");

        if (await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUserId) != null)
            throw new BadHttpRequestException(string.Format("User {0} is already liked by user {1}", likedUserId, sourceUserId));

        var userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUserId
        };
        sourceUser.LikedUsers.Add(userLike);

        if (!await _unitOfWork.Complete()) throw new BadHttpRequestException(string.Format("Unexpected error occured while user {0} tried to like {1}", likedUserId, sourceUserId));
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        return await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
    }
}
