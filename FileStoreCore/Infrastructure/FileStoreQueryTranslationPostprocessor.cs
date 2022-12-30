using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryTranslationPostprocessor : QueryTranslationPostprocessor
{
    public FileStoreQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext) 
        : base(dependencies, queryCompilationContext)
    {
    }
}