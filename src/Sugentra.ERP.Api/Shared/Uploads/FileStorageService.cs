using Dapper;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Shared.Uploads;

public class FileStorageService(
    GenericRepository<UploadFile> repository,
    IDbConnectionFactory connectionFactory,
    IWebHostEnvironment environment,
    ICurrentUserService currentUserService,
    IConfiguration configuration) : IFileStorageService
{
    private string RootFolder => configuration["Uploads:RootFolder"] ?? "uploads";

    // First few bytes of common file formats - checked against the actual upload, not just its extension/name,
    // so a renamed malicious file (e.g. a script saved as "photo.jpg") is rejected instead of trusted blindly.
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]],
        [".pdf"] = [[0x25, 0x50, 0x44, 0x46]],
        [".doc"] = [[0xD0, 0xCF, 0x11, 0xE0]],
        [".docx"] = [[0x50, 0x4B, 0x03, 0x04]],
        [".xls"] = [[0xD0, 0xCF, 0x11, 0xE0]],
        [".xlsx"] = [[0x50, 0x4B, 0x03, 0x04]],
        [".mp4"] = [[0x00, 0x00, 0x00, 0x18], [0x00, 0x00, 0x00, 0x1C], [0x00, 0x00, 0x00, 0x20]],
        [".mov"] = [[0x00, 0x00, 0x00, 0x14]],
    };

    public async Task<UploadFile> SaveAsync(IFormFile file, string category, string? entityType = null, long? entityId = null, string? replaceUrl = null)
    {
        if (!UploadCategories.Rules.TryGetValue(category, out var rule))
        {
            throw new FileValidationException($"Unknown upload category '{category}'.");
        }

        if (file.Length == 0)
        {
            throw new FileValidationException("File is empty.");
        }

        if (file.Length > rule.MaxSizeBytes)
        {
            throw new FileValidationException($"File exceeds the maximum allowed size of {rule.MaxSizeBytes / 1024 / 1024} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!rule.AllowedExtensions.Contains(extension))
        {
            throw new FileValidationException($"File extension '{extension}' is not allowed for category '{category}'.");
        }

        await ValidateSignatureAsync(file, extension);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeDir = Path.Combine(RootFolder, category);
        var absoluteDir = Path.Combine(environment.WebRootPath, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, storedFileName);
        await using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var upload = new UploadFile
        {
            Category = category,
            EntityType = entityType,
            EntityId = entityId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            Extension = extension,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            Url = "/" + Path.Combine(relativeDir, storedFileName).Replace('\\', '/'),
            StoragePath = absolutePath,
            StorageProvider = "Local",
            IsPublic = rule.IsPublic,
            CreatedBy = currentUserService.UserId,
        };

        await repository.AddAsync(upload);

        if (!string.IsNullOrEmpty(replaceUrl))
        {
            await DeleteByUrlAsync(replaceUrl);
        }

        return upload;
    }

    // Removes the file/row being replaced so a re-upload doesn't leave the old one behind as orphaned junk.
    private async Task DeleteByUrlAsync(string url)
    {
        using var connection = connectionFactory.CreateConnection();
        var previous = await connection.QuerySingleOrDefaultAsync<UploadFile>(
            "SELECT * FROM Shared_Uploads WHERE Url = @Url AND IsDeleted = 0", new { Url = url });

        if (previous is null)
        {
            return;
        }

        if (File.Exists(previous.StoragePath))
        {
            File.Delete(previous.StoragePath);
        }

        await repository.SoftDeleteAsync(previous.Id, currentUserService.UserId ?? 0);
    }

    public async Task DeleteAsync(long uploadId)
    {
        var upload = await repository.GetByIdAsync(uploadId);
        if (upload is null)
        {
            return;
        }

        if (File.Exists(upload.StoragePath))
        {
            File.Delete(upload.StoragePath);
        }

        await repository.SoftDeleteAsync(uploadId, currentUserService.UserId ?? 0);
    }

    private static async Task ValidateSignatureAsync(IFormFile file, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var candidates))
        {
            return;
        }

        var header = new byte[candidates.Max(s => s.Length)];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header);
        stream.Position = 0;

        var matches = candidates.Any(signature => read >= signature.Length && header.Take(signature.Length).SequenceEqual(signature));
        if (!matches)
        {
            throw new FileValidationException("File content does not match its extension.");
        }
    }
}
