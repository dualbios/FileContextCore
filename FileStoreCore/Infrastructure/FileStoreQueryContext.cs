using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

internal class FileStoreQueryContext : QueryContext
{
    public FileStoreQueryContext(QueryContextDependencies dependencies, IFileStoreStore store)
        : base(dependencies)
    {
        Store = store;
    }

    public IFileStoreStore Store { get; }
}