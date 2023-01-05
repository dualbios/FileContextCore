using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure;

public partial class FileStoreShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly Type _contextType;
    private readonly bool _threadSafetyChecksEnabled;

    public FileStoreShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies, 
        QueryCompilationContext queryCompilationContext) 
        : base(dependencies, queryCompilationContext)
    {
        _contextType = queryCompilationContext.ContextType;
        _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
    }

    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var inMemoryQueryExpression = (FileStoreQueryExpression)shapedQueryExpression.QueryExpression;
        inMemoryQueryExpression.ApplyProjection();

        var shaperExpression = new ShaperExpressionProcessingExpressionVisitor(
                this, inMemoryQueryExpression, QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            .ProcessShaper(shapedQueryExpression.ShaperExpression);
        var innerEnumerable = Visit(inMemoryQueryExpression.ServerQueryExpression);

        return Expression.New(
            typeof(QueryingEnumerable<>).MakeGenericType(shaperExpression.ReturnType).GetConstructors()[0],
            QueryCompilationContext.QueryContextParameter,
            innerEnumerable,
            Expression.Constant(shaperExpression.Compile()),
            Expression.Constant(_contextType),
            Expression.Constant(
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            Expression.Constant(_threadSafetyChecksEnabled));
    }
}