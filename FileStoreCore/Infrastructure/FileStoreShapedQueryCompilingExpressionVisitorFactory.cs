using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;

    public FileStoreShapedQueryCompilingExpressionVisitorFactory(ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        return new FileStoreShapedQueryCompilingExpressionVisitor(_dependencies, queryCompilationContext);
    }
}