using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FileStoreCore.Infrastructure;

public class FileStoreQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
{
    public QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        throw new NotImplementedException();
    }
}