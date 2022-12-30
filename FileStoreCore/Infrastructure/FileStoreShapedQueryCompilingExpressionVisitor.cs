using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public class FileStoreShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    public FileStoreShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies, 
        QueryCompilationContext queryCompilationContext) 
        : base(dependencies, queryCompilationContext)
    {
    }

    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        throw new NotImplementedException();
    }
}