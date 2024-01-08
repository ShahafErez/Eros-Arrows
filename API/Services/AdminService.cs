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

        var user = await _userManager.FindByNameAsync(username);
        if (user == null) throw new KeyNotFoundException();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
        if (!result.Succeeded) throw new BadHttpRequestException("Failed to add to roles");
        result = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
        if (!result.Succeeded) throw new BadHttpRequestException("Failed to remove from roles");

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
