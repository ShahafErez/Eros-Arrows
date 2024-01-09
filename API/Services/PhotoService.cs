using API.DTOs;
using API.Entities;
using API.Errors;
using API.Helpers;
using API.Interfaces;
using API.Interfaces.Service;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{

    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config, IPhotoService photoService, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _photoService = photoService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;

        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<PhotoDto> addPhoto(IFormFile file, string username)
    {
        Task<User> user = getUserFromUsername(username);
        var addingResult = await AddPhotoAsync(file);
        var photo = new Photo
        {
            Url = addingResult.SecureUrl.AbsoluteUri,
            PublicId = addingResult.PublicId
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

    private
    public Task<MemberDto> getUser(string username)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    public async Task SetMainPhoto(int photoId)
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

    public async Task DeletePhoto(int photoId)
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

    private async Task<User> getUserFromUsername(string username)
    {
        return await _unitOfWork.UserRepository.GetUserByUsernameAsync(username)
        ?? throw new NotFoundException(string.Format("User {0} was not found", username));
    }

    private async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "da-net7"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        if (uploadResult.Error != null) throw new BadHttpRequestException(uploadResult.Error.Message);

        return uploadResult;
    }

    private async void AssignPhotoToUser(User user, Photo photo)
    {
        // The photo will be set as main, if there are no other photos
        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);
        if (await _unitOfWork.Complete())
        {
            var headerValue = new { username = user.UserName };
            var createdObject = _mapper.Map<PhotoDto>(photo);
            return createdObject;
            // TODO- check return value
            // return CreatedAtAction(nameof(getUser), headerValue, createdObject);
        }
        return BadRequest("Problem adding photo");
    }

    private async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
