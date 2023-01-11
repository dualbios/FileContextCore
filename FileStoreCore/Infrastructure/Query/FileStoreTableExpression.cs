// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure.Query;

public class FileStoreTableExpression : Expression, IPrintableExpression
{
    public FileStoreTableExpression(IEntityType entityType)
    {
        EntityType = entityType;
    }

    public override Type Type
        => typeof(IEnumerable<ValueBuffer>);

    public virtual IEntityType EntityType { get; }

    public override sealed ExpressionType NodeType
        => ExpressionType.Extension;

    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(nameof(FileStoreTableExpression) + ": Entity: " + EntityType.DisplayName());
}