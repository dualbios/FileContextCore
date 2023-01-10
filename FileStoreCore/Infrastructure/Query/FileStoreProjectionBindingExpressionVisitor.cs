// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace FileStoreCore.Infrastructure.Query.Internal;

public class FileStoreProjectionBindingExpressionVisitor : ExpressionVisitor
{
    private readonly FileStoreQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly FileStoreExpressionTranslatingExpressionVisitor _expressionTranslatingExpressionVisitor;

    private FileStoreQueryExpression _queryExpression;
    private bool _indexBasedBinding;

    private Dictionary<EntityProjectionExpression, ProjectionBindingExpression> _entityProjectionCache;

    private readonly Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private List<Expression> _clientProjections;
    private readonly Stack<ProjectionMember> _projectionMembers = new();

    public FileStoreProjectionBindingExpressionVisitor(
        FileStoreQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        FileStoreExpressionTranslatingExpressionVisitor expressionTranslatingExpressionVisitor)
    {
        _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        _expressionTranslatingExpressionVisitor = expressionTranslatingExpressionVisitor;
        _queryExpression = null!;
    }

    public virtual Expression Translate(FileStoreQueryExpression queryExpression, Expression expression)
    {
        _queryExpression = queryExpression;
        _indexBasedBinding = false;

        _projectionMembers.Push(new ProjectionMember());
        var result = Visit(expression);

        if (result == QueryCompilationContext.NotTranslatedExpression)
        {
            _indexBasedBinding = true;
            _projectionMapping.Clear();
            _entityProjectionCache = new Dictionary<EntityProjectionExpression, ProjectionBindingExpression>();
            _clientProjections = new List<Expression>();

            result = Visit(expression);

            _queryExpression.ReplaceProjection(_clientProjections);
            _clientProjections = null;
        }
        else
        {
            _queryExpression.ReplaceProjection(_projectionMapping);
            _projectionMapping.Clear();
        }

        _queryExpression = null!;
        _projectionMembers.Clear();
        result = MatchTypes(result, expression.Type);

        return result;
    }

    [return: NotNullIfNotNull("expression")]
    public override Expression Visit(Expression expression)
    {
        if (expression == null)
        {
            return null;
        }

        if (!(expression is NewExpression
                || expression is MemberInitExpression
                || expression is EntityShaperExpression
                || expression is IncludeExpression))
        {
            if (_indexBasedBinding)
            {
                switch (expression)
                {
                    case ConstantExpression:
                        return expression;

                    case ProjectionBindingExpression projectionBindingExpression:
                        var mappedProjection = _queryExpression.GetProjection(projectionBindingExpression);
                        if (mappedProjection is EntityProjectionExpression entityProjection)
                        {
                            return AddClientProjection(entityProjection, typeof(ValueBuffer));
                        }

                        if (mappedProjection is not FileStoreQueryExpression)
                        {
                            return AddClientProjection(mappedProjection, expression.Type.MakeNullable());
                        }

                        throw new InvalidOperationException("CoreStrings.TranslationFailed(projectionBindingExpression.Print())");

                    case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                        {
                            var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                materializeCollectionNavigationExpression.Subquery)!;
                            _clientProjections!.Add(subquery.QueryExpression);
                            return new CollectionResultShaperExpression(
                                new ProjectionBindingExpression(
                                    _queryExpression, _clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>)),
                                subquery.ShaperExpression,
                                materializeCollectionNavigationExpression.Navigation,
                                materializeCollectionNavigationExpression.Navigation.ClrType.GetSequenceType());
                        }

                    case MethodCallExpression methodCallExpression:
                        if (methodCallExpression.Method.IsGenericMethod
                            && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                            && methodCallExpression.Method.Name == nameof(Enumerable.ToList)
                            && methodCallExpression.Arguments.Count == 1
                            && methodCallExpression.Arguments[0].Type.TryGetElementType(typeof(IQueryable<>)) != null)
                        {
                            var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                methodCallExpression.Arguments[0]);
                            if (subquery != null)
                            {
                                _clientProjections!.Add(subquery.QueryExpression);
                                return new CollectionResultShaperExpression(
                                    new ProjectionBindingExpression(
                                        _queryExpression, _clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>)),
                                    subquery.ShaperExpression,
                                    null,
                                    methodCallExpression.Method.GetGenericArguments()[0]);
                            }
                        }
                        else
                        {
                            var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
                            if (subquery != null)
                            {
                                // This simplifies the check when subquery is translated and can be lifted as scalar.
                                var scalarTranslation = _expressionTranslatingExpressionVisitor.Translate(subquery);
                                if (scalarTranslation != null)
                                {
                                    return AddClientProjection(scalarTranslation, expression.Type.MakeNullable());
                                }

                                if (subquery.ResultCardinality == ResultCardinality.Enumerable)
                                {
                                    _clientProjections!.Add(subquery.QueryExpression);
                                    var projectionBindingExpression = new ProjectionBindingExpression(
                                        _queryExpression, _clientProjections.Count - 1, typeof(IEnumerable<ValueBuffer>));
                                    return new CollectionResultShaperExpression(
                                        projectionBindingExpression, subquery.ShaperExpression, navigation: null,
                                        subquery.ShaperExpression.Type);
                                }
                                else
                                {
                                    _clientProjections!.Add(subquery.QueryExpression);
                                    var projectionBindingExpression = new ProjectionBindingExpression(
                                        _queryExpression, _clientProjections.Count - 1, typeof(ValueBuffer));
                                    return new SingleResultShaperExpression(projectionBindingExpression, subquery.ShaperExpression);
                                }
                            }
                        }

                        break;
                }

                var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                return translation != null
                    ? AddClientProjection(translation, expression.Type.MakeNullable())
                    : base.Visit(expression);
            }
            else
            {
                var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                if (translation == null)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                _projectionMapping[_projectionMembers.Peek()] = translation;

                return new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), expression.Type.MakeNullable());
            }
        }

        return base.Visit(expression);
    }

    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        var left = MatchTypes(Visit(binaryExpression.Left), binaryExpression.Left.Type);
        var right = MatchTypes(Visit(binaryExpression.Right), binaryExpression.Right.Type);

        return binaryExpression.Update(left, VisitAndConvert(binaryExpression.Conversion, "VisitBinary"), right);
    }

    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        if (test.Type == typeof(bool?))
        {
            test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
        }

        ifTrue = MatchTypes(ifTrue, conditionalExpression.IfTrue.Type);
        ifFalse = MatchTypes(ifFalse, conditionalExpression.IfFalse.Type);

        return conditionalExpression.Update(test, ifTrue, ifFalse);
    }

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is EntityShaperExpression entityShaperExpression)
        {
            EntityProjectionExpression entityProjectionExpression;
            if (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                entityProjectionExpression =
                    (EntityProjectionExpression)((FileStoreQueryExpression)projectionBindingExpression.QueryExpression)
                    .GetProjection(projectionBindingExpression);
            }
            else
            {
                entityProjectionExpression = (EntityProjectionExpression)entityShaperExpression.ValueBufferExpression;
            }

            if (_indexBasedBinding)
            {
                if (!_entityProjectionCache!.TryGetValue(entityProjectionExpression, out var entityProjectionBinding))
                {
                    entityProjectionBinding = AddClientProjection(entityProjectionExpression, typeof(ValueBuffer));
                    _entityProjectionCache[entityProjectionExpression] = entityProjectionBinding;
                }

                return entityShaperExpression.Update(entityProjectionBinding);
            }

            _projectionMapping[_projectionMembers.Peek()] = entityProjectionExpression;

            return entityShaperExpression.Update(
                new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
        }

        if (extensionExpression is IncludeExpression includeExpression)
        {
            return _indexBasedBinding
                ? base.VisitExtension(includeExpression)
                : QueryCompilationContext.NotTranslatedExpression;
        }

        throw new InvalidOperationException("CoreStrings.TranslationFailed(extensionExpression.Print())");
    }

    protected override ElementInit VisitElementInit(ElementInit elementInit)
    {
        return elementInit.Update(elementInit.Arguments.Select(e => MatchTypes(Visit(e), e.Type)));
    }

    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var expression = Visit(memberExpression.Expression);
        Expression updatedMemberExpression = memberExpression.Update(
            expression != null ? MatchTypes(expression, memberExpression.Expression!.Type) : expression);

        if (expression?.Type.IsNullableValueType() == true)
        {
            var nullableReturnType = memberExpression.Type.MakeNullable();
            if (!memberExpression.Type.IsNullableType())
            {
                updatedMemberExpression = Expression.Convert(updatedMemberExpression, nullableReturnType);
            }

            updatedMemberExpression = Expression.Condition(
                Expression.Equal(expression, Expression.Default(expression.Type)),
                Expression.Constant(null, nullableReturnType),
                updatedMemberExpression);
        }

        return updatedMemberExpression;
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        var expression = memberAssignment.Expression;
        Expression visitedExpression;
        if (_indexBasedBinding)
        {
            visitedExpression = Visit(memberAssignment.Expression);
        }
        else
        {
            var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
            _projectionMembers.Push(projectionMember);

            visitedExpression = Visit(memberAssignment.Expression);
            if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return memberAssignment.Update(Expression.Convert(visitedExpression, memberAssignment.Expression.Type));
            }

            _projectionMembers.Pop();
        }

        visitedExpression = MatchTypes(visitedExpression, expression.Type);

        return memberAssignment.Update(visitedExpression);
    }

    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        var newExpression = Visit(memberInitExpression.NewExpression);
        if (newExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
        for (var i = 0; i < newBindings.Length; i++)
        {
            if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);
            if (((MemberAssignment)newBindings[i]).Expression is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Operand == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return memberInitExpression.Update((NewExpression)newExpression, newBindings);
    }

    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var @object = Visit(methodCallExpression.Object);
        var arguments = new Expression[methodCallExpression.Arguments.Count];
        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var argument = methodCallExpression.Arguments[i];
            arguments[i] = MatchTypes(Visit(argument), argument.Type);
        }

        Expression updatedMethodCallExpression = methodCallExpression.Update(
            @object != null ? MatchTypes(@object, methodCallExpression.Object!.Type) : @object!,
            arguments);

        if (@object?.Type.IsNullableType() == true
            && !methodCallExpression.Object!.Type.IsNullableType())
        {
            var nullableReturnType = methodCallExpression.Type.MakeNullable();
            if (!methodCallExpression.Type.IsNullableType())
            {
                updatedMethodCallExpression = Expression.Convert(updatedMethodCallExpression, nullableReturnType);
            }

            return Expression.Condition(
                Expression.Equal(@object, Expression.Default(@object.Type)),
                Expression.Constant(null, nullableReturnType),
                updatedMethodCallExpression);
        }

        return updatedMethodCallExpression;
    }

    protected override Expression VisitNew(NewExpression newExpression)
    {
        if (newExpression.Arguments.Count == 0)
        {
            return newExpression;
        }

        if (!_indexBasedBinding
            && newExpression.Members == null)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newArguments = new Expression[newExpression.Arguments.Count];
        for (var i = 0; i < newArguments.Length; i++)
        {
            var argument = newExpression.Arguments[i];
            Expression visitedArgument;
            if (_indexBasedBinding)
            {
                visitedArgument = Visit(argument);
            }
            else
            {
                var projectionMember = _projectionMembers.Peek().Append(newExpression.Members![i]);
                _projectionMembers.Push(projectionMember);
                visitedArgument = Visit(argument);
                if (visitedArgument == QueryCompilationContext.NotTranslatedExpression)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                _projectionMembers.Pop();
            }

            newArguments[i] = MatchTypes(visitedArgument, argument.Type);
        }

        return newExpression.Update(newArguments);
    }

    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        return newArrayExpression.Update(newArrayExpression.Expressions.Select(e => MatchTypes(Visit(e), e.Type)));
    }

    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var operand = Visit(unaryExpression.Operand);

        return (unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked)
            && unaryExpression.Type == operand.Type
                ? operand
                : unaryExpression.Update(MatchTypes(operand, unaryExpression.Operand.Type));
    }

    private static Expression MatchTypes(Expression expression, Type targetType)
    {
        if (targetType != expression.Type
            && targetType.TryGetElementType(typeof(IQueryable<>)) == null)
        {
            expression = Expression.Convert(expression, targetType);
        }

        return expression;
    }

    private ProjectionBindingExpression AddClientProjection(Expression expression, Type type)
    {
        var existingIndex = _clientProjections!.FindIndex(e => e.Equals(expression));
        if (existingIndex == -1)
        {
            _clientProjections.Add(expression);
            existingIndex = _clientProjections.Count - 1;
        }

        return new ProjectionBindingExpression(_queryExpression, existingIndex, type);
    }
}