using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        if (await _likeService.AddLike(username, User.GetUserId()))
        {
            return Ok();
        }
        throw new Exception("Unexpected error occurred");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> getUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        PagedList<LikeDto> users = await _likeService.getUserLikes(likesParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
        return Ok(users);
    }

}
