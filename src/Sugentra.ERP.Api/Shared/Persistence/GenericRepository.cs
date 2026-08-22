using Sugentra.ERP.Api.Shared.Common;

namespace Sugentra.ERP.Api.Shared.Persistence;

/// <summary>Concrete, instantiable form of <see cref="Repository{TEntity}"/> for simple reference-data entities with no specialized queries.</summary>
public class GenericRepository<TEntity>(IDbConnectionFactory connectionFactory) : Repository<TEntity>(connectionFactory)
    where TEntity : BaseAuditableEntity;
