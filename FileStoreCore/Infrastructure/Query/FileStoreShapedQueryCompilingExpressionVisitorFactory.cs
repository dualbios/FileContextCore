using Microsoft.EntityFrameworkCore.Query;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class FileStoreShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    public FileStoreShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    protected virtual ShapedQueryCompilingExpressionVisitorDependencies Dependencies { get; }

    public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new FileStoreShapedQueryCompilingExpressionVisitor(Dependencies, queryCompilationContext);
}