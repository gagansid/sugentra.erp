using Sugentra.ERP.Api.Modules.Approvals.Dtos;
using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Modules.Approvals.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Approvals.Repositories;

public interface IApprovalRequestRepository
{
    Task<ApprovalRequest?> GetActiveByDocumentAsync(string documentType, long documentId);
    Task<ApprovalRequest?> GetByIdAsync(long id);
    Task<IReadOnlyList<ApprovalRequestLevel>> GetLevelsAsync(long requestId);
    Task<IReadOnlyList<ApprovalRequestLevelApprover>> GetApproversAsync(long requestLevelId);
    Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalRequestLevelApprover>>> GetApproversByRequestLevelIdsAsync(IEnumerable<long> requestLevelIds);
    Task<IReadOnlyList<ApprovalRequestAction>> GetHistoryAsync(long requestId);
    Task<ApprovalRequestLevel?> GetCurrentLevelAsync(long requestId, int levelNumber);
    Task<bool> IsEligibleApproverAsync(long requestLevelId, long userId);
    Task<bool> HasAlreadyActedAsync(long requestId, int levelNumber, long userId);
    Task MarkApproverActedAsync(long requestLevelId, long userId);
    Task<IReadOnlyList<ApprovalInboxItem>> GetInboxForUserAsync(long userId);
}

public class ApprovalRequestRepository(IDbConnectionFactory connectionFactory) : IApprovalRequestRepository
{
    public async Task<ApprovalRequest?> GetActiveByDocumentAsync(string documentType, long documentId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<ApprovalRequest>(
            ApprovalQueries.GetActiveRequestByDocumentSql, new { DocumentType = documentType, DocumentId = documentId });
    }

    public async Task<ApprovalRequest?> GetByIdAsync(long id)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<ApprovalRequest>(ApprovalQueries.GetRequestByIdSql, new { Id = id });
    }

    public async Task<IReadOnlyList<ApprovalRequestLevel>> GetLevelsAsync(long requestId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalRequestLevel>(ApprovalQueries.GetLevelsByRequestIdSql, new { RequestId = requestId });
    }

    public async Task<IReadOnlyList<ApprovalRequestLevelApprover>> GetApproversAsync(long requestLevelId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalRequestLevelApprover>(
            ApprovalQueries.GetApproversByRequestLevelIdSql, new { RequestLevelId = requestLevelId });
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalRequestLevelApprover>>> GetApproversByRequestLevelIdsAsync(IEnumerable<long> requestLevelIds)
    {
        var ids = requestLevelIds as IReadOnlyCollection<long> ?? requestLevelIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, IReadOnlyList<ApprovalRequestLevelApprover>>();

        using var connection = connectionFactory.CreateConnection();
        var approvers = await connection.QueryListAsync<ApprovalRequestLevelApprover>(
            ApprovalQueries.GetApproversByRequestLevelIdsSql, new { RequestLevelIds = ids });
        return approvers.GroupBy(a => a.RequestLevelId).ToDictionary(g => g.Key, g => (IReadOnlyList<ApprovalRequestLevelApprover>)g.ToList());
    }

    public async Task<IReadOnlyList<ApprovalRequestAction>> GetHistoryAsync(long requestId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalRequestAction>(ApprovalQueries.GetActionsByRequestIdSql, new { RequestId = requestId });
    }

    public async Task<ApprovalRequestLevel?> GetCurrentLevelAsync(long requestId, int levelNumber)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<ApprovalRequestLevel>(
            ApprovalQueries.GetCurrentLevelSql, new { RequestId = requestId, LevelNumber = levelNumber });
    }

    public async Task<bool> IsEligibleApproverAsync(long requestLevelId, long userId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(
            ApprovalQueries.IsEligibleApproverSql, new { RequestLevelId = requestLevelId, UserId = userId }) > 0;
    }

    public async Task<bool> HasAlreadyActedAsync(long requestId, int levelNumber, long userId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryScalarAsync<int>(
            ApprovalQueries.HasAlreadyActedSql, new { RequestId = requestId, LevelNumber = levelNumber, UserId = userId }) > 0;
    }

    public async Task MarkApproverActedAsync(long requestLevelId, long userId)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteCommandAsync(ApprovalQueries.MarkApproverActedSql, new { RequestLevelId = requestLevelId, UserId = userId });
    }

    public async Task<IReadOnlyList<ApprovalInboxItem>> GetInboxForUserAsync(long userId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalInboxItem>(ApprovalQueries.GetInboxForUserSql, new { UserId = userId });
    }
}
