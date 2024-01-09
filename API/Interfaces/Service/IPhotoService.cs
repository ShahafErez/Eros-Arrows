using API.DTOs;

namespace API.Interfaces.Service;

public interface IPhotoService
{
    Task<PhotoDto> AddPhoto(IFormFile file, string username);
    Task<MemberDto> GetUser(string username);
    Task SetMainPhoto(int photoId, string username);
    Task DeletePhoto(int photoId, string username);
}
