using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryContextFactory: IQueryContextFactory
{
    private readonly QueryContextDependencies _dependencies;

    public FileStoreQueryContextFactory(QueryContextDependencies dependencies)
    {
        _dependencies = dependencies;
    }
    public QueryContext Create()
    {
        return new FileStoreQueryContext(_dependencies);
    }
}