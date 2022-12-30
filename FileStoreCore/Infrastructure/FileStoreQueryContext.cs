using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryContext : QueryContext
{
    public FileStoreQueryContext(QueryContextDependencies dependencies) : base(dependencies)
    {
    }
}