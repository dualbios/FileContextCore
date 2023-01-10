using FileStoreCore.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public class FileStoreDatabase : Database, IFileStoreDatabase
{
    private readonly IFileStoreStore _store;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly IUpdateAdapterFactory _updateAdapterFactory;

    public FileStoreDatabase(
        DatabaseDependencies dependencies,
        IFileStoreStoreCache storeCache,
        IDbContextOptions options,
        IDesignTimeModel designTimeModel,
        IUpdateAdapterFactory updateAdapterFactory) : base(dependencies)
    {
        _store = storeCache.GetStore(options);
        _designTimeModel = designTimeModel;
        _updateAdapterFactory = updateAdapterFactory;
    }

    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        return _store.ExecuteTransaction(entries);
    }

    public override Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = new CancellationToken())
    {
        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<int>(cancellationToken)
            : Task.FromResult(_store.ExecuteTransaction(entries));
    }

    public virtual bool EnsureDatabaseCreated()
    {
        return _store.EnsureCreated(_updateAdapterFactory, _designTimeModel.Model);
    }
}