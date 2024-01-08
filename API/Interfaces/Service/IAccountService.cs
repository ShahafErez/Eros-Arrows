using API.DTOs;

namespace API.Interfaces.Service;

public interface IAccountService
{
    Task<UserDto> Register(RegisterDto registerDto);
    Task<UserDto> Login(LoginDto loginDto);
}
