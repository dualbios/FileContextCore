using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreDatabaseCreator : IDatabaseCreator
{
    private readonly IDatabase _database;

    public FileStoreDatabaseCreator(IDatabase database)
    {
        _database = database;
    }

    protected virtual FileStoreDatabase Database => (FileStoreDatabase)_database;

    public bool CanConnect()
    {
        return true;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }

    public bool EnsureCreated()
    {
        return true;
    }

    public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }

    public bool EnsureDeleted()
    {
        return true;
    }

    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }
}