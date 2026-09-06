using Sugentra.ERP.Api.Modules.Inventory.Entities;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Inventory.Repositories;

public interface ILandedCostDocumentLineRepository
{
    Task<IReadOnlyList<LandedCostDocumentLine>> GetByDocumentIdAsync(long documentId);
    Task<long> AddAsync(LandedCostDocumentLine entity);
}

public class LandedCostDocumentLineRepository(IDbConnectionFactory connectionFactory)
    : Repository<LandedCostDocumentLine>(connectionFactory), ILandedCostDocumentLineRepository
{
    public async Task<IReadOnlyList<LandedCostDocumentLine>> GetByDocumentIdAsync(long documentId)
    {
        using var connection = ConnectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Inventory_LandedCostDocumentLines WHERE LandedCostDocumentId = @LandedCostDocumentId AND IsDeleted = 0";
        return await connection.QueryListAsync<LandedCostDocumentLine>(sql, new { LandedCostDocumentId = documentId });
    }
}
