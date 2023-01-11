﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure.Query;

public partial class FileStoreQueryExpression
{
    private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
    {
        private readonly Func<ValueBuffer> _getElement;

        public ResultEnumerable(Func<ValueBuffer> getElement)
        {
            _getElement = getElement;
        }

        public IEnumerator<ValueBuffer> GetEnumerator()
        {
            return new ResultEnumerator(_getElement());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
        {
            private readonly ValueBuffer _value;
            private bool _moved;

            public ResultEnumerator(ValueBuffer value)
            {
                _value = value;
                _moved = _value.IsEmpty;
            }

            public bool MoveNext()
            {
                if (!_moved)
                {
                    _moved = true;

                    return _moved;
                }

                return false;
            }

            public void Reset()
            {
                _moved = false;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public ValueBuffer Current
            {
                get { return !_moved ? ValueBuffer.Empty : _value; }
            }

            void IDisposable.Dispose()
            {
            }
        }
    }

    private sealed class ProjectionMemberRemappingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _queryExpression;
        private readonly Dictionary<ProjectionMember, ProjectionMember> _projectionMemberMappings;

        public ProjectionMemberRemappingExpressionVisitor(
            Expression queryExpression,
            Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
        {
            _queryExpression = queryExpression;
            _projectionMemberMappings = projectionMemberMappings;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression Visit(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                return new ProjectionBindingExpression(
                    _queryExpression,
                    _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionMemberToIndexConvertingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _queryExpression;
        private readonly Dictionary<ProjectionMember, int> _projectionMemberMappings;

        public ProjectionMemberToIndexConvertingExpressionVisitor(
            Expression queryExpression,
            Dictionary<ProjectionMember, int> projectionMemberMappings)
        {
            _queryExpression = queryExpression;
            _projectionMemberMappings = projectionMemberMappings;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression Visit(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                return new ProjectionBindingExpression(
                    _queryExpression,
                    _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionIndexRemappingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldExpression;
        private readonly Expression _newExpression;
        private readonly int[] _indexMap;

        public ProjectionIndexRemappingExpressionVisitor(
            Expression oldExpression,
            Expression newExpression,
            int[] indexMap)
        {
            _oldExpression = oldExpression;
            _newExpression = newExpression;
            _indexMap = indexMap;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression Visit(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression
                && ReferenceEquals(projectionBindingExpression.QueryExpression, _oldExpression))
            {
                return new ProjectionBindingExpression(
                    _newExpression,
                    _indexMap[projectionBindingExpression.Index.Value],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            return extensionExpression is EntityShaperExpression entityShaper
                ? entityShaper.MakeNullable()
                : base.VisitExtension(extensionExpression);
        }
    }

    private sealed class QueryExpressionReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldQuery;
        private readonly Expression _newQuery;

        public QueryExpressionReplacingExpressionVisitor(Expression oldQuery, Expression newQuery)
        {
            _oldQuery = oldQuery;
            _newQuery = newQuery;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression Visit(Expression expression)
        {
            return expression is ProjectionBindingExpression projectionBindingExpression
                   && ReferenceEquals(projectionBindingExpression.QueryExpression, _oldQuery)
                ? projectionBindingExpression.ProjectionMember != null
                    ? new ProjectionBindingExpression(
                        _newQuery, projectionBindingExpression.ProjectionMember!, projectionBindingExpression.Type)
                    : new ProjectionBindingExpression(
                        _newQuery, projectionBindingExpression.Index!.Value, projectionBindingExpression.Type)
                : base.Visit(expression);
        }
    }

    private sealed class CloningExpressionVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull("expression")]
        public override Expression Visit(Expression expression)
        {
            if (expression is FileStoreQueryExpression inMemoryQueryExpression)
            {
                var clonedInMemoryQueryExpression = new FileStoreQueryExpression(
                    inMemoryQueryExpression.ServerQueryExpression, inMemoryQueryExpression._valueBufferParameter)
                {
                    _groupingParameter = inMemoryQueryExpression._groupingParameter,
                    _singleResultMethodInfo = inMemoryQueryExpression._singleResultMethodInfo,
                    _scalarServerQuery = inMemoryQueryExpression._scalarServerQuery
                };

                clonedInMemoryQueryExpression._clientProjections.AddRange(
                    inMemoryQueryExpression._clientProjections.Select(e => Visit(e)));
                clonedInMemoryQueryExpression._projectionMappingExpressions.AddRange(
                    inMemoryQueryExpression._projectionMappingExpressions);
                foreach (var (projectionMember, value) in inMemoryQueryExpression._projectionMapping)
                {
                    clonedInMemoryQueryExpression._projectionMapping[projectionMember] = Visit(value);
                }

                return clonedInMemoryQueryExpression;
            }

            if (expression is EntityProjectionExpression entityProjectionExpression)
            {
                return entityProjectionExpression.Clone();
            }

            return base.Visit(expression);
        }
    }
}