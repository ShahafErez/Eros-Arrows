using API.Interfaces.Service;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountService(UserManager<User> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<UserDto> Login(LoginDto loginDto)
    {
        var user = await validateUsernameAndPassword(loginDto.Username.ToLower(), loginDto.Password);

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<User> validateUsernameAndPassword(string username, string password)
    {
        string unauthorizedErrorMessage = "Username or Password is incorrect.";
        var user = await _userManager.Users
        .Include(p => p.Photos)
        .SingleOrDefaultAsync(user => user.UserName == username) ??
        throw new UnauthorizedAccessException(unauthorizedErrorMessage);

        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordCorrect) throw new UnauthorizedAccessException(unauthorizedErrorMessage);

        return user;
    }

    public async Task<UserDto> Register(RegisterDto registerDto)
    {
        var username = registerDto.Username.ToLower();
        if (await UserExists(username)) throw new BadHttpRequestException("Username is taken");

        var user = _mapper.Map<User>(registerDto);
        user.UserName = username;

        createUserWithDefaultRole(user, registerDto.Password);

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(user => user.UserName == username.ToLower());
    }

    private async void createUserWithDefaultRole(User user, string password)
    {
        var creationResult = await _userManager.CreateAsync(user, password);
        if (!creationResult.Succeeded) throw new Exception(creationResult.Errors.ToString());

        var roleAssignmentResult = await _userManager.AddToRoleAsync(user, "Member");
        if (!roleAssignmentResult.Succeeded) throw new Exception(creationResult.Errors.ToString());
    }
}
