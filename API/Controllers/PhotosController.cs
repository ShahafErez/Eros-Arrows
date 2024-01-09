using API.DTOs;
using API.Extensions;
using API.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class PhotosController : BaseApiController
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpPost]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        return await _photoService.AddPhoto(file, User.GetUsername());
    }


    [HttpPut("set-main/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        await _photoService.SetMainPhoto(photoId, User.GetUsername());
        return Ok();
    }

    [HttpDelete("{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        await _photoService.DeletePhoto(photoId, User.GetUsername());
        return Ok();
    }
}
