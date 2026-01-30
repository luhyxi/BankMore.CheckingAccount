namespace SharedKernel;
public interface IRepository<T> where T : class, IAggregateRoot
{
    ValueTask CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    ValueTask<Guid> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    ValueTask<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);
    
    ValueTask<T> GetAsync(Guid id, CancellationToken cancellationToken = default);
    
    ValueTask<T> UpdateAsync(Guid id, T entity, CancellationToken cancellationToken = default);
}