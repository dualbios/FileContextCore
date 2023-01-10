// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FileStoreCore.Infrastructure.Query.Internal;

public class SingleResultShaperExpression : Expression, IPrintableExpression
{
    public SingleResultShaperExpression(
        Expression projection,
        Expression innerShaper)
    {
        Projection = projection;
        InnerShaper = innerShaper;
        Type = innerShaper.Type;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var projection = visitor.Visit(Projection);
        var innerShaper = visitor.Visit(InnerShaper);

        return Update(projection, innerShaper);
    }

    public virtual SingleResultShaperExpression Update(Expression projection, Expression innerShaper)
        => projection != Projection || innerShaper != InnerShaper
            ? new SingleResultShaperExpression(projection, innerShaper)
            : this;

    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    public override Type Type { get; }

    public virtual Expression Projection { get; }

    public virtual Expression InnerShaper { get; }

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"{nameof(SingleResultShaperExpression)}:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("(");
            expressionPrinter.Visit(Projection);
            expressionPrinter.Append(", ");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.AppendLine(")");
        }
    }
}
