using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Shared.ErrorLogging;

public class ErrorLogService(GenericRepository<ErrorLog> repository) : IErrorLogService
{
    public async Task LogAsync(ErrorLogEntry entry)
    {
        // Best-effort: failing to persist the error log must never mask/replace the original error.
        try
        {
            await repository.AddAsync(new ErrorLog
            {
                Source = entry.Source,
                OccurredAt = DateTime.UtcNow,
                Endpoint = Truncate(entry.Endpoint, 500),
                HttpMethod = entry.HttpMethod,
                StatusCode = entry.StatusCode,
                ExceptionType = Truncate(entry.ExceptionType, 255),
                Message = entry.Message,
                StackTrace = entry.StackTrace,
                QueryString = Truncate(entry.QueryString, 1000),
                RequestParameters = entry.RequestParameters,
                UserId = entry.UserId,
                Username = Truncate(entry.Username, 100),
                IpAddress = Truncate(entry.IpAddress, 45)
            });
        }
        catch
        {
            // Swallow - error logging itself must not throw and hide the real exception.
        }
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];
}
