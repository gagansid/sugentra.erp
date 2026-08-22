namespace Sugentra.ERP.Api.Shared.Contracts;

/// <summary>Cross-module lookup for generic, admin-editable app-wide config values (e.g. PasswordReset's
/// ExpiresMinutes) — other modules (e.g. Identity) depend on this interface only, never on
/// Modules/Settings/** directly, per the module isolation rule in AGENTS.md.</summary>
public interface ISystemParameterService
{
    /// <summary>Returns the integer value of <paramref name="key"/> (optionally scoped to <paramref name="category"/>),
    /// or <paramref name="defaultValue"/> if not configured/parseable.</summary>
    Task<int> GetIntAsync(string? category, string key, int defaultValue);

    /// <summary>Returns the string value of <paramref name="key"/> (optionally scoped to <paramref name="category"/>),
    /// or <paramref name="defaultValue"/> if not configured.</summary>
    Task<string?> GetStringAsync(string? category, string key, string? defaultValue = null);
}
