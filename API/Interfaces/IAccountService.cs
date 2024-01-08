using API.DTOs;

namespace AP.interfaces;

public interface IAccountService
{
    Task<UserDto> Register(RegisterDto registerDto);
    Task<UserDto> Login(LoginDto loginDto);
}
