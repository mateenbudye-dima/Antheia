using Microsoft.EntityFrameworkCore;

namespace Antheia.Infrastructure.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    /// Attaches an entity to the ChangeTracker if it isn't already tracked locally.
    /// </summary>
    public static void AttachStubIfMissing<TEntity>(
        this DbContext context,
        Func<TEntity, bool> predicate,
        Func<TEntity> factory) where TEntity : class
    {
        if (!context.Set<TEntity>().Local.Any(predicate))
        {
            var stub = factory();
            context.Set<TEntity>().Attach(stub);
        }
    }
}