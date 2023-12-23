using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> getUsers()
    {
        return Ok(await _userRepository.GetUsersAsync());
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<User>> getUser(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }
}
