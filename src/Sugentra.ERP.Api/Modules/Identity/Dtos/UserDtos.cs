using System.ComponentModel.DataAnnotations;

namespace Sugentra.ERP.Api.Modules.Identity.Dtos;

public record CreateUserRequest(
    [Required, StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username may only contain letters, digits, dot, underscore and hyphen.")]
    string Username,

    [Required, StringLength(256), RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must be a valid address with a domain and extension (e.g. user@example.com).")]
    string Email,

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string Password,

    [Required, StringLength(200, MinimumLength = 2)]
    string FullName,

    [Phone(ErrorMessage = "Phone number is not a valid phone number."), StringLength(50)]
    string? PhoneNumber = null,

    [StringLength(50)]
    string? EmployeeId = null);

public record UpdateUserRequest(
    [Required, StringLength(256), RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must be a valid address with a domain and extension (e.g. user@example.com).")]
    string Email,

    [Required, StringLength(200, MinimumLength = 2)]
    string FullName,

    bool IsActive,

    [Phone(ErrorMessage = "Phone number is not a valid phone number."), StringLength(50)]
    string? PhoneNumber = null,

    [StringLength(50)]
    string? EmployeeId = null,

    string? ProfilePictureUrl = null);

public record ChangePasswordRequest(
    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string NewPassword);

public record UserResponse(long Id, string Username, string Email, string FullName, bool IsActive, string? PhoneNumber, string? EmployeeId, string? ProfilePictureUrl, DateTime? LastLoginAt, DateTime CreatedAt, DateTime? EmailVerifiedAt = null, DateTime? PhoneVerifiedAt = null);

public record UserListItemDto(long Id, string Username, string Email, string FullName, bool IsActive, DateTime? LockoutEnd, bool IsBanned);

// Bound as one model from query string (?Username=&Email=&FullName=&Status=&Page=&PageSize=) — filters are optional.
// Status: Active | Inactive | Locked | Banned; null = all.
public record UserListRequest(string? Username = null, string? Email = null, string? FullName = null, string? Status = null, int Page = 1, int PageSize = 20);

public record SetPermissionOverrideRequest(bool IsAllowed);
