using Sugentra.ERP.Api.Modules.Identity.Dtos;
using Sugentra.ERP.Api.Modules.Identity.Entities;
using Sugentra.ERP.Api.Modules.Identity.Repositories;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Common;
using Sugentra.ERP.Api.Shared.Logging;

namespace Sugentra.ERP.Api.Modules.Identity.UseCases;

public class UserUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService)
{
    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (await userRepository.ExistsByUsernameAsync(request.Username))
        {
            return Result<UserResponse>.Failure($"Username '{request.Username}' is already taken.");
        }

        if (await userRepository.ExistsByEmailAsync(request.Email))
        {
            return Result<UserResponse>.Failure($"Email '{request.Email}' is already registered.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedBy = currentUserService.UserId ?? 0
        };

        var id = await userRepository.AddAsync(user);

        await auditLogService.LogAsync("Identity_Users", id, "Create", null, $"Username={user.Username}", currentUserService.UserId ?? 0);

        return Result<UserResponse>.Success(ToResponse(user));
    }

    public async Task<Result<UserResponse>> UpdateAsync(long id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<UserResponse>.Failure("User not found.");
        }

        var oldValues = $"Email={user.Email};FullName={user.FullName};IsActive={user.IsActive}";

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.IsActive = request.IsActive;
        user.UpdatedBy = currentUserService.UserId;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);

        var newValues = $"Email={user.Email};FullName={user.FullName};IsActive={user.IsActive}";
        await auditLogService.LogAsync("Identity_Users", id, "Update", oldValues, newValues, currentUserService.UserId ?? 0);

        return Result<UserResponse>.Success(ToResponse(user));
    }

    public async Task<Result<bool>> DeactivateAsync(long id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return Result<bool>.Failure("User not found.");
        }

        await userRepository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync("Identity_Users", id, "SoftDelete", null, null, currentUserService.UserId ?? 0);

        return Result<bool>.Success(true);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Username, user.Email, user.FullName, user.IsActive, user.CreatedAt);
}
