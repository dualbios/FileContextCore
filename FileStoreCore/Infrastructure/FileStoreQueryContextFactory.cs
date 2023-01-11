using kDg.FileBaseContext.Extensions;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreQueryContextFactory : IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;
    private readonly IFileStoreStore _store;

    public FileStoreQueryContextFactory(
        QueryContextDependencies dependencies,
        IFileStoreStoreCache fileStoreStoreCache,
        IDbContextOptions contextOptions)
    {
        //_store = storeCache.GetStore(contextOptions);
        _dependencies = dependencies;
        _store = fileStoreStoreCache.GetStore(contextOptions);
    }

    public virtual QueryContext Create()
    {
        return new FileStoreQueryContext(_dependencies, _store);
    }
}