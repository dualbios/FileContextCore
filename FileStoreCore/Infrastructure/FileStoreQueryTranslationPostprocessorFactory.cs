using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;

    public FileStoreQueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new FileStoreQueryTranslationPostprocessor(_dependencies, queryCompilationContext);
    }
}