using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface IUserService
{
    Task<PagedList<MemberDto>> GetUsers(UserParams userParams, string username);
    Task<MemberDto> GetUser(string username);
    Task UpdateUser(MemberUpdateDto memberUpdateDto, string username);
}
