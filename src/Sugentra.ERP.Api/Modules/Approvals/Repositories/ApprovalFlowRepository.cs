using Sugentra.ERP.Api.Modules.Approvals.Entities;
using Sugentra.ERP.Api.Modules.Approvals.Queries;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Modules.Approvals.Repositories;

public interface IApprovalFlowRepository
{
    Task<IReadOnlyList<ApprovalFlowDefinition>> GetActiveByApproverTypeAsync(string approverType);
    Task<IReadOnlyList<ApprovalFlowLevel>> GetLevelsAsync(long flowDefinitionId);
    Task<IReadOnlyList<ApprovalFlowLevelApprover>> GetApproversAsync(long flowLevelId);
    Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevel>>> GetLevelsByFlowDefinitionIdsAsync(IEnumerable<long> flowDefinitionIds);
    Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevelApprover>>> GetApproversByFlowLevelIdsAsync(IEnumerable<long> flowLevelIds);
    Task ReplaceLevelsAsync(long flowDefinitionId, IReadOnlyList<ApprovalFlowLevel> levels, IReadOnlyDictionary<int, List<ApprovalFlowLevelApprover>> approversByLevelNumber);
    Task<bool> IsRoleAuthorizedForApproverTypeAsync(long roleId, string approverType);
}

public class ApprovalFlowRepository(
    IDbConnectionFactory connectionFactory,
    GenericRepository<ApprovalFlowLevel> levelRepository,
    GenericRepository<ApprovalFlowLevelApprover> approverRepository) : IApprovalFlowRepository
{
    public async Task<IReadOnlyList<ApprovalFlowDefinition>> GetActiveByApproverTypeAsync(string approverType)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalFlowDefinition>(
            ApprovalQueries.GetActiveFlowDefinitionsByApproverTypeSql, new { ApproverType = approverType });
    }

    public async Task<IReadOnlyList<ApprovalFlowLevel>> GetLevelsAsync(long flowDefinitionId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalFlowLevel>(
            ApprovalQueries.GetLevelsByFlowDefinitionIdSql, new { FlowDefinitionId = flowDefinitionId });
    }

    public async Task<IReadOnlyList<ApprovalFlowLevelApprover>> GetApproversAsync(long flowLevelId)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryListAsync<ApprovalFlowLevelApprover>(
            ApprovalQueries.GetApproversByFlowLevelIdSql, new { FlowLevelId = flowLevelId });
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevel>>> GetLevelsByFlowDefinitionIdsAsync(IEnumerable<long> flowDefinitionIds)
    {
        var ids = flowDefinitionIds as IReadOnlyCollection<long> ?? flowDefinitionIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, IReadOnlyList<ApprovalFlowLevel>>();

        using var connection = connectionFactory.CreateConnection();
        var levels = await connection.QueryListAsync<ApprovalFlowLevel>(ApprovalQueries.GetLevelsByFlowDefinitionIdsSql, new { FlowDefinitionIds = ids });
        return levels.GroupBy(l => l.FlowDefinitionId).ToDictionary(g => g.Key, g => (IReadOnlyList<ApprovalFlowLevel>)g.ToList());
    }

    public async Task<IReadOnlyDictionary<long, IReadOnlyList<ApprovalFlowLevelApprover>>> GetApproversByFlowLevelIdsAsync(IEnumerable<long> flowLevelIds)
    {
        var ids = flowLevelIds as IReadOnlyCollection<long> ?? flowLevelIds.ToList();
        if (ids.Count == 0) return new Dictionary<long, IReadOnlyList<ApprovalFlowLevelApprover>>();

        using var connection = connectionFactory.CreateConnection();
        var approvers = await connection.QueryListAsync<ApprovalFlowLevelApprover>(ApprovalQueries.GetApproversByFlowLevelIdsSql, new { FlowLevelIds = ids });
        return approvers.GroupBy(a => a.FlowLevelId).ToDictionary(g => g.Key, g => (IReadOnlyList<ApprovalFlowLevelApprover>)g.ToList());
    }

    public async Task ReplaceLevelsAsync(
        long flowDefinitionId,
        IReadOnlyList<ApprovalFlowLevel> levels,
        IReadOnlyDictionary<int, List<ApprovalFlowLevelApprover>> approversByLevelNumber)
    {
        using var connection = connectionFactory.CreateConnection();
        var existingLevels = await connection.QueryListAsync<ApprovalFlowLevel>(
            ApprovalQueries.GetLevelsByFlowDefinitionIdSql, new { FlowDefinitionId = flowDefinitionId });
        foreach (var existingLevel in existingLevels)
        {
            await connection.ExecuteCommandAsync(ApprovalQueries.DeleteApproversByFlowLevelIdSql, new { FlowLevelId = existingLevel.Id });
        }
        await connection.ExecuteCommandAsync(ApprovalQueries.DeleteLevelsByFlowDefinitionIdSql, new { FlowDefinitionId = flowDefinitionId });

        foreach (var level in levels)
        {
            level.FlowDefinitionId = flowDefinitionId;
            var levelId = await levelRepository.AddAsync(level);

            if (approversByLevelNumber.TryGetValue(level.LevelNumber, out var approvers))
            {
                foreach (var approver in approvers)
                {
                    approver.FlowLevelId = levelId;
                    await approverRepository.AddAsync(approver);
                }
            }
        }
    }

    public async Task<bool> IsRoleAuthorizedForApproverTypeAsync(long roleId, string approverType)
    {
        using var connection = connectionFactory.CreateConnection();
        var count = await connection.QueryScalarAsync<int>(
            ApprovalQueries.IsRoleAuthorizedForApproverTypeSql, new { RoleId = roleId, ApproverType = approverType });
        return count > 0;
    }
}
