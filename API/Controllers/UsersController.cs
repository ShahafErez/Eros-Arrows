﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Authorize]
public class UsersController : BaseApiController
{
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IPhotoService photoService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _photoService = photoService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> getUsers([FromQuery] UserParams userParams)
    {
        var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
        userParams.CurrentUsername = User.GetUsername();

        if (string.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = gender == "male" ? "female" : "male";
        }

        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> getUser(string username)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> updateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();
        _mapper.Map(memberUpdateDto, user);
        if (await _unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> addPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound();

        var result = await _photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        // if it's the first photo by the user, we'll set it as admin
        if (user.Photos.Count == 0) photo.IsMain = true;
        user.Photos.Add(photo);
        if (await _unitOfWork.Complete())
        {
            var headerValue = new { username = user.UserName };
            var createdObject = _mapper.Map<PhotoDto>(photo);
            return CreatedAtAction(nameof(getUser), headerValue, createdObject);
        }
        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound();
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null) return NotFound();
        if (photo.IsMain) return BadRequest(string.Format("Photo with ID {0} is already set as the main photo", photoId));
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();
        return BadRequest("problem setting the new photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound();
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null) return NotFound();
        if (photo.IsMain) return BadRequest("Cannot delete main photo");
        if (photo.PublicId != null)
        {
            // delete from cloudnairy
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);
        if (await _unitOfWork.Complete()) return Ok();
        return BadRequest("Problem deleting photo");
    }
}
