using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreTableExpression : Expression, IPrintableExpression
{
    public FileStoreTableExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    public override Type Type => typeof(IEnumerable<ValueBuffer>);

    public virtual IEntityType EntityType { get; }

    public override sealed ExpressionType NodeType => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        return this;
    }

    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(nameof(FileStoreTableExpression) + ": Entity: " + EntityType.DisplayName());
    }
}