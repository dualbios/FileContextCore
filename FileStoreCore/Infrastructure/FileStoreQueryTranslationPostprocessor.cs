using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
    public FileStoreQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
    }
}