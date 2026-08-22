using Sugentra.ERP.Api.Modules.Settings.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Settings.Repositories;

public record NextDocumentNumberResult(int CurrentNumber, string FormattedNumber);

public interface IDocumentNumberingRepository
{
    Task<DocumentNumbering?> GetByIdAsync(long id);
    Task<IReadOnlyList<DocumentNumbering>> GetAllAsync();
    Task<long> AddAsync(DocumentNumbering entity);
    Task UpdateAsync(DocumentNumbering entity);
    Task SoftDeleteAsync(long id, long deletedBy);
    Task<NextDocumentNumberResult> GetNextAsync(string documentType);
}

public class DocumentNumberingRepository(IDbConnectionFactory connectionFactory)
    : GenericRepository<DocumentNumbering>(connectionFactory), IDocumentNumberingRepository
{
    public async Task<NextDocumentNumberResult> GetNextAsync(string documentType)
    {
        using var connection = ConnectionFactory.CreateConnection();
        var result = await connection.QuerySingleStoredProcedureAsync<NextDocumentNumberResult>(
            "usp_Setting_DocumentNumbering_GetNext", new { DocumentType = documentType });

        return result ?? throw new InvalidOperationException($"DocumentType '{documentType}' is not configured.");
    }
}
