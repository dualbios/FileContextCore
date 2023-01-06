using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;
using FileStoreCore.Storage;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;
    private readonly IFileStoreStore _fileStoreStore;

    public FileStoreQueryContextFactory(
        QueryContextDependencies dependencies,
        IFileStoreStore fileStoreStore,
        IDbContextOptions contextOptions)
    {
        //_store = storeCache.GetStore(contextOptions);
        _dependencies = dependencies;
        _fileStoreStore = fileStoreStore;
    }

    public virtual QueryContext Create()
    {
        return new FileStoreQueryContext(_dependencies, _fileStoreStore);
    }
}