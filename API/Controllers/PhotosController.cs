using API.DTOs;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class PhotosController : BaseApiController
{
    private readonly PhotoService _photoService;

    public PhotosController(PhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpPost]
    public async Task<ActionResult<PhotoDto>> addPhoto(IFormFile file)
    {
        return await _photoService.addPhoto(file, User.GetUsername());
    }

    public async Task<ActionResult<MemberDto>> getUser(string username)
    {
        return await _photoService.getUser(username);
    }

    [HttpPut("set-main/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        return await _photoService.SetMainPhoto(photoId);
    }

    [HttpDelete("{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        return await _photoService.DeletePhoto(photoId);
    }
}
