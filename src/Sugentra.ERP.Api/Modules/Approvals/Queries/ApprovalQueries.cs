namespace Sugentra.ERP.Api.Modules.Approvals.Queries;

// SQL text only — no execution here (Queries convention, see AGENTS.md).
public static class ApprovalQueries
{
    public const string GetActiveFlowDefinitionsByDocumentTypeSql = """
        SELECT * FROM Approval_FlowDefinitions
        WHERE DocumentType = @DocumentType AND IsActive = 1 AND IsDeleted = 0
        ORDER BY Priority DESC, Id ASC
        """;

    public const string GetLevelsByFlowDefinitionIdSql = """
        SELECT * FROM Approval_FlowLevels WHERE FlowDefinitionId = @FlowDefinitionId AND IsDeleted = 0 ORDER BY LevelNumber ASC
        """;

    public const string GetApproversByFlowLevelIdSql = """
        SELECT * FROM Approval_FlowLevelApprovers WHERE FlowLevelId = @FlowLevelId AND IsDeleted = 0
        """;

    public const string GetLevelsByFlowDefinitionIdsSql = """
        SELECT * FROM Approval_FlowLevels WHERE FlowDefinitionId IN @FlowDefinitionIds AND IsDeleted = 0 ORDER BY FlowDefinitionId, LevelNumber ASC
        """;

    public const string GetApproversByFlowLevelIdsSql = """
        SELECT * FROM Approval_FlowLevelApprovers WHERE FlowLevelId IN @FlowLevelIds AND IsDeleted = 0
        """;

    public const string DeleteLevelsByFlowDefinitionIdSql = "DELETE FROM Approval_FlowLevels WHERE FlowDefinitionId = @FlowDefinitionId";

    public const string DeleteApproversByFlowLevelIdSql = "DELETE FROM Approval_FlowLevelApprovers WHERE FlowLevelId = @FlowLevelId";

    public const string GetActiveRequestByDocumentSql = """
        SELECT TOP 1 * FROM Approval_Requests
        WHERE DocumentType = @DocumentType AND DocumentId = @DocumentId AND Status = 'Pending' AND IsDeleted = 0
        ORDER BY Id DESC
        """;

    public const string GetRequestByIdSql = "SELECT * FROM Approval_Requests WHERE Id = @Id AND IsDeleted = 0";

    public const string GetLevelsByRequestIdSql = """
        SELECT * FROM Approval_RequestLevels WHERE RequestId = @RequestId AND IsDeleted = 0 ORDER BY LevelNumber ASC
        """;

    public const string GetApproversByRequestLevelIdSql = """
        SELECT * FROM Approval_RequestLevelApprovers WHERE RequestLevelId = @RequestLevelId AND IsDeleted = 0
        """;

    public const string GetApproversByRequestLevelIdsSql = """
        SELECT * FROM Approval_RequestLevelApprovers WHERE RequestLevelId IN @RequestLevelIds AND IsDeleted = 0
        """;

    public const string GetActionsByRequestIdSql = """
        SELECT * FROM Approval_RequestActions WHERE RequestId = @RequestId AND IsDeleted = 0 ORDER BY ActionedAt ASC
        """;

    public const string GetCurrentLevelSql = """
        SELECT * FROM Approval_RequestLevels
        WHERE RequestId = @RequestId AND LevelNumber = @LevelNumber AND IsDeleted = 0
        """;

    public const string MarkApproverActedSql = """
        UPDATE Approval_RequestLevelApprovers SET HasActed = 1 WHERE RequestLevelId = @RequestLevelId AND UserId = @UserId
        """;

    public const string IsEligibleApproverSql = """
        SELECT COUNT(1) FROM Approval_RequestLevelApprovers
        WHERE RequestLevelId = @RequestLevelId AND UserId = @UserId AND IsDeleted = 0
        """;

    public const string HasAlreadyActedSql = """
        SELECT COUNT(1) FROM Approval_RequestActions
        WHERE RequestId = @RequestId AND LevelNumber = @LevelNumber AND ApproverUserId = @UserId
        """;

    public const string IsRoleAuthorizedForDocumentTypeSql = """
        SELECT COUNT(1) FROM Approval_RoleCategories
        WHERE RoleId = @RoleId AND DocumentType = @DocumentType AND IsDeleted = 0
        """;

    // Inbox: requests currently pending on the given user at their current level (not yet acted).
    public const string GetInboxForUserSql = """
        SELECT r.Id AS RequestId, r.DocumentType, r.DocumentId, r.DocumentNumber, r.CurrentLevelNumber,
               rl.Name AS LevelName, r.RequestedBy, r.RequestedAt
        FROM Approval_Requests r
        JOIN Approval_RequestLevels rl ON rl.RequestId = r.Id AND rl.LevelNumber = r.CurrentLevelNumber AND rl.IsDeleted = 0
        JOIN Approval_RequestLevelApprovers rla ON rla.RequestLevelId = rl.Id AND rla.IsDeleted = 0
        WHERE r.Status = 'Pending' AND r.IsDeleted = 0 AND rla.UserId = @UserId AND rla.HasActed = 0
        ORDER BY r.RequestedAt ASC
        """;
}
