using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<User> _userManager;

    public AdminService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IList<string>> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles)) throw new BadHttpRequestException("You must select at least one role");
        var selectedRoles = roles.Split(",").ToArray();

        var user = await _userManager.FindByNameAsync(username) ??
        throw new KeyNotFoundException(string.Format("User by the username {0} was not found", username));

        var currentRoles = await _userManager.GetRolesAsync(user);

        var addingResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
        if (!addingResult.Succeeded)
            throw new BadHttpRequestException("Failed to add roles to user " + username);

        var removingResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
        if (!removingResult.Succeeded)
            throw new BadHttpRequestException("Failed to remove roles from user " + username);

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<List<UserRoleDto>> GetUsersWithRoles()
    {
        return await _userManager.Users
         .OrderBy(u => u.UserName)
         .Select(u => new UserRoleDto
         {
             Id = u.Id,
             Username = u.UserName,
             Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
         }).ToListAsync();

    }
}
