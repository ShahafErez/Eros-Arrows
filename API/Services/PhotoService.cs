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

    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;

        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(acc);
    }

    public async Task<PhotoDto> AddPhoto(IFormFile file, string username)
    {
        User user = await GetUserByUsername(username);
        var addingResult = await AddPhotoAsync(file);

        var photo = new Photo
        {
            Url = addingResult.SecureUrl.AbsoluteUri,
            PublicId = addingResult.PublicId
        };

        return await AssignPhotoToUser(user, photo);
    }

    public async Task<MemberDto> GetUser(string username)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    public async Task SetMainPhoto(int photoId, string username)
    {
        User user = await GetUserByUsername(username);
        Photo photo = GetPhoto(user, photoId);

        if (photo.IsMain) throw new BadHttpRequestException(string.Format("Photo {0} is already set as the main photo", photoId));

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (!await _unitOfWork.Complete()) throw new BadHttpRequestException("problem setting the new photo");
    }

    public async Task DeletePhoto(int photoId, string username)
    {
        User user = await GetUserByUsername(username);
        Photo photo = GetPhoto(user, photoId);

        if (photo.IsMain) throw new BadHttpRequestException("Cannot delete main photo");
        if (photo.PublicId != null)
        {
            var result = await DeletePhotoFromCloudinary(photo.PublicId);
            if (result.Error != null) throw new BadHttpRequestException(result.Error.Message);
        }
        user.Photos.Remove(photo);

        if (!await _unitOfWork.Complete()) throw new BadHttpRequestException("An unexpected problem occured");
    }

    private async Task<User> GetUserByUsername(string username)
    {
        return await _unitOfWork.UserRepository.GetUserByUsernameAsync(username)
        ?? throw new NotFoundException(string.Format("User {0} was not found", username));
    }

    private static Photo GetPhoto(User user, int photoId)
    {
        return user.Photos.FirstOrDefault(x => x.Id == photoId)
        ?? throw new NotFoundException(string.Format("Photo {0} was not found for user {1}", photoId, user.UserName));
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

    private async Task<PhotoDto> AssignPhotoToUser(User user, Photo photo)
    {
        // The photo will be set as main, if there are no other photos
        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);
        if (!await _unitOfWork.Complete())
        {
            throw new BadHttpRequestException("An unexpected problem occured");
        }
        return _mapper.Map<PhotoDto>(photo);
    }

    private async Task<DeletionResult> DeletePhotoFromCloudinary(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
