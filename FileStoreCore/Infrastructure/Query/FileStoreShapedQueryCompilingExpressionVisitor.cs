using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure.Query;

public partial class FileStoreShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private static readonly MethodInfo TableMethodInfo
        = typeof(FileStoreShapedQueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(Table))!;

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

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case FileStoreTableExpression inMemoryTableExpression:
                return Expression.Call(
                    TableMethodInfo,
                    QueryCompilationContext.QueryContextParameter,
                    Expression.Constant(inMemoryTableExpression.EntityType));
        }

        return base.VisitExtension(extensionExpression);
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

    private static IEnumerable<ValueBuffer> Table(QueryContext queryContext, IEntityType entityType)
    {
        return ((FileStoreQueryContext)queryContext).Store
            .GetTables(entityType)
            .SelectMany(t => t.Rows.Select(vs => new ValueBuffer(vs)));
    }
}