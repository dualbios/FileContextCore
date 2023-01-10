using Microsoft.EntityFrameworkCore.Storage;

namespace FileStoreCore.Extensions;

public class FileStoreTransactionManager : IDbContextTransactionManager/*, ITransactionEnlistmentManager*/
{
    private static readonly FileStoreTransaction _stubTransaction = new FileStoreTransaction();

    public IDbContextTransaction CurrentTransaction { get; } = null;

    public IDbContextTransaction BeginTransaction()
    {
        return _stubTransaction;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult<IDbContextTransaction>(_stubTransaction);
    }

    public void CommitTransaction()
    {
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public void ResetState()
    {
    }

    public Task ResetStateAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public void RollbackTransaction()
    {
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}