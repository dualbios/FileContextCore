using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileStoreCore.Infrastructure;

public class FileStoreDatabaseCreator : IDatabaseCreator
{
    private readonly IDatabase _database;

    protected virtual FileStoreDatabase Database => (FileStoreDatabase)_database;

    public FileStoreDatabaseCreator(IDatabase database)
    {
        _database = database;
    }
    public bool EnsureDeleted()
    {
        return true;
    }

    public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
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

    public bool CanConnect()
    {
        return true;
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(true);
    }
}