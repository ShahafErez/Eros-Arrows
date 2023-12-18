using System.Security.Cryptography;
using System.Text;
using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    public AccountController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto registerDto)
    {
        var username = registerDto.Username.ToLower();
        if (await UserExists(username)) return BadRequest("Username is taken");

        var password = registerDto.Password;

        using var hmac = new HMACSHA512();
        var user = new User
        {
            UserName = username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
            PasswordSalt = hmac.Key
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }
}
