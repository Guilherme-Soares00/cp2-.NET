using Microsoft.EntityFrameworkCore;
using Recommenda.Application.Interfaces.Repositories;
using Recommenda.Domain.Common;

namespace Recommenda.Infrastructure.Persistence.Repositories;

public sealed class Repository<T>(RecommendaContext context) : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet
            .AsNoTracking()
            .OrderBy(entity => entity.CreatedAt)
            .ThenBy(entity => entity.Id)
            .ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbSet.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var existing = await _dbSet.FindAsync([entity.Id], cancellationToken);
        if (existing is null)
            return null;

        // Atualiza a instância já rastreada para evitar dois objetos com a mesma chave.
        context.Entry(existing).CurrentValues.SetValues(entity);
        existing.MarkUpdated();
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        if (entity is null)
            return false;

        _dbSet.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
