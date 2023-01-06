using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

internal class FileStoreQueryContext : QueryContext
{
    public IFileStoreStore Store { get; }

    public FileStoreQueryContext(QueryContextDependencies dependencies, IFileStoreStore store) 
        : base(dependencies)
    {
        Store = store;
    }
}