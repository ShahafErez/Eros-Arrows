using API.DTOs;

namespace API.Interfaces.Service;

public interface IPhotoService
{
    // Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    // Task<DeletionResult> DeletePhotoAsync(string publicId);
    Task<PhotoDto> addPhoto(IFormFile file);
    Task<MemberDto> getUser(string username);
    Task SetMainPhoto(int photoId);
    Task DeletePhoto(int photoId);
}
