using API.DTOs;

namespace API.Interfaces.Service;

public interface IAdminService
{

    Task<List<UserRoleDto>> GetUsersWithRoles();
    Task<IList<string>> EditRoles(string username, string roles);

}
