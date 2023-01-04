using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FileStoreCore.Infrastructure;

public class FileStoreTableExpression : Expression, IPrintableExpression
{
    public FileStoreTableExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    public override Type Type => typeof(IEnumerable<ValueBuffer>);

    public virtual IEntityType EntityType { get; }

    public sealed override ExpressionType NodeType => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        return this;
    }

    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(nameof(FileStoreTableExpression) + ": Entity: " + EntityType.DisplayName());
    }
}