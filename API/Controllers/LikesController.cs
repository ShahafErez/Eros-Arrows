﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public LikesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username.ToLower());
        var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

        if (likedUser == null) return NotFound(string.Format("User {0} was not found", username));
        var likedUserId = likedUser.Id;

        if (sourceUser.UserName == username) return BadRequest("A user cannot like it own profile");

        if (await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUserId) != null)
            return BadRequest(string.Format("User {0} is already liked by user {1}", username, User.GetUsername()));

        var userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUserId
        };
        sourceUser.LikedUsers.Add(userLike);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest(string.Format("Unexpected error occured while user {0} tried to like {1}", User.GetUsername(), username));
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> getUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
        return Ok(users);
    }

}
