using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class FileStoreQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
{
    public FileStoreQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new FileStoreQueryableMethodTranslatingExpressionVisitor(Dependencies, queryCompilationContext);
}