using API.Entities;

namespace API;

public interface IAdminService
{

    Task<User[]> GetUsersWithRoles();
    Task<IList<string>> EditRoles(string username, string roles);

}
