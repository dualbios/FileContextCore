// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace FileStoreCore.Infrastructure.Query.Internal;

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