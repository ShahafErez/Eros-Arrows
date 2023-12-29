using API.Controllers;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class LikesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _userRepository.GetUserByUsernameAsync(username.ToLower());
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        if (likedUser == null) return NotFound(String.Format("User {0} was not found", username));
        var likedUserId = likedUser.Id;

        if (sourceUser.UserName == username) return BadRequest("A user cannot like it own profile");

        if (await _likesRepository.GetUserLike(sourceUserId, likedUserId) != null)
            return BadRequest(String.Format("User {0} is already liked by user {1}", likedUserId, sourceUserId));

        var userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUserId
        };
        sourceUser.LikedUsers.Add(userLike);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest(String.Format("Failed to add like to user {0} by user {1}", likedUserId, sourceUserId));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> getUserLikes(string predicate)
    {
        var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId());
        return Ok(users);
    }

}
