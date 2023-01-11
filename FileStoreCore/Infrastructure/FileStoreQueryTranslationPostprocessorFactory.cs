using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
{
    private readonly QueryTranslationPostprocessorDependencies _dependencies;

    public FileStoreQueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        return new FileStoreQueryTranslationPostprocessor(_dependencies, queryCompilationContext);
    }
}