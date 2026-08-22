using System.Text.Json;
using Sugentra.ERP.Api.Shared.Auth;
using Sugentra.ERP.Api.Shared.Logging;
using Sugentra.ERP.Api.Shared.Persistence;

namespace Sugentra.ERP.Api.Shared.Common;

/// <summary>
/// Shared CRUD flow (audit logging + audit columns) for reference/config entities with no bespoke business rules.
/// Bespoke UseCases (e.g. Identity's User/Role) still write their own class when uniqueness checks or extra
/// side effects are needed — this is only for the "plain CRUD" tier described in AGENTS.md.
/// </summary>
public class CrudUseCase<TEntity>(
    GenericRepository<TEntity> repository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUserService,
    string tableName)
    where TEntity : BaseAuditableEntity
{
    public Task<TEntity?> GetByIdAsync(long id) => repository.GetByIdAsync(id);

    public Task<IReadOnlyList<TEntity>> GetAllAsync() => repository.GetAllAsync();

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        entity.CreatedBy = currentUserService.UserId;
        var id = await repository.AddAsync(entity);
        await auditLogService.LogAsync(tableName, id, "Create", null, JsonSerializer.Serialize(entity), currentUserService.UserId);
        return entity;
    }

    public async Task<Result<TEntity>> UpdateAsync(long id, TEntity entity)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Result<TEntity>.Failure($"Record {id} not found.");
        }

        var oldValues = JsonSerializer.Serialize(existing);

        entity.Id = id;
        entity.CreatedAt = existing.CreatedAt;
        entity.CreatedBy = existing.CreatedBy;
        entity.UpdatedBy = currentUserService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(entity);
        await auditLogService.LogAsync(tableName, id, "Update", oldValues, JsonSerializer.Serialize(entity), currentUserService.UserId);

        return Result<TEntity>.Success(entity);
    }

    public async Task<Result<bool>> DeleteAsync(long id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
        {
            return Result<bool>.Failure($"Record {id} not found.");
        }

        await repository.SoftDeleteAsync(id, currentUserService.UserId ?? 0);
        await auditLogService.LogAsync(tableName, id, "SoftDelete", JsonSerializer.Serialize(existing), null, currentUserService.UserId);

        return Result<bool>.Success(true);
    }
}
