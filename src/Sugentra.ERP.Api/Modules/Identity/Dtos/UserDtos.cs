namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record CreateUserRequest(string Username, string Email, string Password, string FullName);

public record UpdateUserRequest(string Email, string FullName, bool IsActive);

public record UserResponse(long Id, string Username, string Email, string FullName, bool IsActive, DateTime CreatedAt);

public record UserListItemDto(long Id, string Username, string Email, string FullName, bool IsActive);
