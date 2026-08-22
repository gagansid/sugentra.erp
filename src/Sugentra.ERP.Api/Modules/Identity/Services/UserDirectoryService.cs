using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Contracts;

namespace Sugentra.ERP.Api.Modules.Identity.Services;

public class UserDirectoryService(IUserRepository userRepository) : IUserDirectoryService
{
    public async Task<UserDirectoryEntry?> GetByIdAsync(long id)
    {
        var user = await userRepository.GetByIdAsync(id);
        return user is null ? null : new UserDirectoryEntry(user.Id, user.Username, user.FullName, user.Email);
    }
}
