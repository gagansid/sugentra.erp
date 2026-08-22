namespace Sugentra.ERP.Api.Shared.Uploads;

public interface IFileStorageService
{
    /// <summary>Validates, stores on disk and records metadata for an uploaded file. Throws <see cref="FileValidationException"/> on any rule violation.
    /// If <paramref name="replaceUrl"/> is given, the upload it points to is deleted (file + row) once the new one is saved.</summary>
    Task<UploadFile> SaveAsync(IFormFile file, string category, string? entityType = null, long? entityId = null, string? replaceUrl = null);

    Task DeleteAsync(long uploadId);
}

/// <summary>Thrown for any client-facing validation failure (bad category, extension, size, or content mismatch).</summary>
public class FileValidationException(string message) : Exception(message);
