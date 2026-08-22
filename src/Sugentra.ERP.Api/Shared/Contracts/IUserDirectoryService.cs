namespace Sugentra.ERP.Api.Shared.Contracts;

public record UserDirectoryEntry(long Id, string Username, string FullName, string Email);

/// <summary>Cross-module read-only lookup of basic user identity info (e.g. for Cc'ing the sender of a
/// notification email) — other modules depend on this interface only, never on Modules/Identity/** directly,
/// per the module isolation rule in AGENTS.md.</summary>
public interface IUserDirectoryService
{
    Task<UserDirectoryEntry?> GetByIdAsync(long id);
}
