﻿//using FileStoreCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.EntityFrameworkCore.Query;
//using Microsoft.EntityFrameworkCore.Storage;
//using System.Linq.Expressions;
//using System.Reflection;
//using Microsoft.EntityFrameworkCore.Utilities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Microsoft.EntityFrameworkCore.Query.Internal;
//using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

//namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
//    internal class FileStoreQueryExpression : Expression, IPrintableExpression
//    {
//        private static readonly ConstructorInfo ValueBufferConstructor
//            = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

//        private static readonly PropertyInfo ValueBufferCountMemberInfo
//            = typeof(ValueBuffer).GetTypeInfo().GetProperty(nameof(ValueBuffer.Count))!;

//        private static readonly MethodInfo LeftJoinMethodInfo = typeof(FileStoreQueryExpression).GetTypeInfo()
//            .GetDeclaredMethods(nameof(LeftJoin)).Single(mi => mi.GetParameters().Length == 6);

//        private static readonly ConstructorInfo ResultEnumerableConstructor
//            = typeof(FileStoreResultEnumerable).GetConstructors().Single();

//        private readonly ParameterExpression _valueBufferParameter;
//        private ParameterExpression? _groupingParameter;
//        private MethodInfo? _singleResultMethodInfo;
//        private bool _scalarServerQuery;

//        private CloningExpressionVisitor? _cloningExpressionVisitor;

//        private Dictionary<ProjectionMember, Expression> _projectionMapping = new();
//        private readonly List<Expression> _clientProjections = new();
//        private readonly List<Expression> _projectionMappingExpressions = new();

//        private FileStoreQueryExpression(
//            Expression serverQueryExpression,
//            ParameterExpression valueBufferParameter)
//        {
//            ServerQueryExpression = serverQueryExpression;
//            _valueBufferParameter = valueBufferParameter;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public FileStoreQueryExpression(IEntityType entityType)
//        {
//            _valueBufferParameter = Parameter(typeof(ValueBuffer), "valueBuffer");
//            ServerQueryExpression = new FileStoreTableExpression(entityType);
//            Dictionary<IProperty, MethodCallExpression> propertyExpressionsMap = new Dictionary<IProperty, MethodCallExpression>();
//            List<Expression> selectorExpressions = new List<Expression>();
//            foreach (IProperty property in entityType.GetAllBaseTypesInclusive().SelectMany(et => et.GetDeclaredProperties()))
//            {
//                MethodCallExpression propertyExpression = CreateReadValueExpression(property.ClrType, property.GetIndex(), property);
//                selectorExpressions.Add(propertyExpression);

//                //Check.DebugAssert(
//                //    property.GetIndex() == selectorExpressions.Count - 1,
//                //    "Properties should be ordered in same order as their indexes.");
//                //propertyExpressionsMap[property] = propertyExpression;
//                //_projectionMappingExpressions.Add(propertyExpression);
//            }

//            IProperty discriminatorProperty = entityType.FindDiscriminatorProperty();
//            if (discriminatorProperty != null)
//            {
//                ValueComparer keyValueComparer = discriminatorProperty.GetKeyValueComparer();
//                foreach (IEntityType derivedEntityType in entityType.GetDerivedTypes())
//                {
//                    Expression entityCheck = derivedEntityType.GetConcreteDerivedTypesInclusive()
//                        .Select(
//                            e => keyValueComparer.ExtractEqualsBody(
//                                propertyExpressionsMap[discriminatorProperty],
//                                Constant(e.GetDiscriminatorValue(), discriminatorProperty.ClrType)))
//                        .Aggregate((l, r) => OrElse(l, r));

//                    foreach (IProperty property in derivedEntityType.GetDeclaredProperties())
//                    {
//                        // We read nullable value from property of derived type since it could be null.
//                        Type typeToRead = property.ClrType.MakeNullable();
//                        ConditionalExpression propertyExpression = Condition(
//                            entityCheck,
//                            CreateReadValueExpression(typeToRead, property.GetIndex(), property),
//                            Default(typeToRead));

//                        selectorExpressions.Add(propertyExpression);
//                        MethodCallExpression readExpression = CreateReadValueExpression(propertyExpression.Type, selectorExpressions.Count - 1, property);
//                        propertyExpressionsMap[property] = readExpression;
//                        _projectionMappingExpressions.Add(readExpression);
//                    }
//                }

//                // Force a selector if entity projection has complex expressions.
//                LambdaExpression selectorLambda = Lambda(
//                    New(
//                        ValueBufferConstructor,
//                        NewArrayInit(
//                            typeof(object),
//                            selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                    CurrentParameter);

//                ServerQueryExpression = Call(
//                    EnumerableMethods.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
//                    ServerQueryExpression,
//                    selectorLambda);
//            }

//            EntityProjectionExpression entityProjection = new EntityProjectionExpression(entityType, propertyExpressionsMap);
//            _projectionMapping[new ProjectionMember()] = entityProjection;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression ServerQueryExpression { get; private set; }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual ParameterExpression CurrentParameter
//            => _groupingParameter ?? _valueBufferParameter;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ReplaceProjection(IReadOnlyList<Expression> clientProjections)
//        {
//            _projectionMapping.Clear();
//            _projectionMappingExpressions.Clear();
//            _clientProjections.Clear();
//            _clientProjections.AddRange(clientProjections);
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ReplaceProjection(IReadOnlyDictionary<ProjectionMember, Expression> projectionMapping)
//        {
//            _projectionMapping.Clear();
//            _projectionMappingExpressions.Clear();
//            _clientProjections.Clear();
//            List<Expression> selectorExpressions = new List<Expression>();
//            foreach ((ProjectionMember projectionMember, Expression expression) in projectionMapping)
//            {
//                if (expression is EntityProjectionExpression entityProjectionExpression)
//                {
//                    _projectionMapping[projectionMember] = AddEntityProjection(entityProjectionExpression);
//                }
//                else
//                {
//                    selectorExpressions.Add(expression);
//                    MethodCallExpression readExpression = CreateReadValueExpression(
//                        expression.Type, selectorExpressions.Count - 1, InferPropertyFromInner(expression));
//                    _projectionMapping[projectionMember] = readExpression;
//                    _projectionMappingExpressions.Add(readExpression);
//                }
//            }

//            if (selectorExpressions.Count == 0)
//            {
//                // No server correlated term in projection so add dummy 1.
//                selectorExpressions.Add(Constant(1));
//            }

//            LambdaExpression selectorLambda = Lambda(
//                New(
//                    ValueBufferConstructor,
//                    NewArrayInit(
//                        typeof(object),
//                        selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
//                CurrentParameter);

//            ServerQueryExpression = Call(
//                EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
//                ServerQueryExpression,
//                selectorLambda);

//            _groupingParameter = null;

//            EntityProjectionExpression AddEntityProjection(EntityProjectionExpression entityProjectionExpression)
//            {
//                Dictionary<IProperty, MethodCallExpression> readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
//                foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//                {
//                    ColumnExpression expression = entityProjectionExpression.BindProperty(property);
//                    selectorExpressions.Add(expression);
//                    MethodCallExpression newExpression = CreateReadValueExpression(expression.Type, selectorExpressions.Count - 1, property);
//                    readExpressionMap[property] = newExpression;
//                    _projectionMappingExpressions.Add(newExpression);
//                }

//                EntityProjectionExpression result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

//                // Also compute nested entity projections
//                foreach (INavigation navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
//                             .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
//                             .SelectMany(t => t.GetDeclaredNavigations()))
//                {
//                    EntityShaperExpression boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
//                    if (boundEntityShaperExpression != null)
//                    {
//                        EntityProjectionExpression innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
//                        EntityProjectionExpression newInnerEntityProjection = AddEntityProjection(innerEntityProjection);
//                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
//                        result.AddNavigationBinding(navigation, boundEntityShaperExpression);
//                    }
//                }

//                return result;
//            }
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression GetProjection(ProjectionBindingExpression projectionBindingExpression)
//            => projectionBindingExpression.ProjectionMember != null
//                ? _projectionMapping[projectionBindingExpression.ProjectionMember]
//                : _clientProjections[projectionBindingExpression.Index!.Value];

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ApplyProjection()
//        {
//            if (_scalarServerQuery)
//            {
//                _projectionMapping[new ProjectionMember()] = Constant(0);
//                return;
//            }

//            List<Expression> selectorExpressions = new List<Expression>();
//            if (_clientProjections.Count > 0)
//            {
//                for (int i = 0; i < _clientProjections.Count; i++)
//                {
//                    Expression projection = _clientProjections[i];
//                    switch (projection)
//                    {
//                        case EntityProjectionExpression entityProjectionExpression:
//                            {
//                                Dictionary<IProperty, int> indexMap = new Dictionary<IProperty, int>();
//                                foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//                                {
//                                    selectorExpressions.Add(entityProjectionExpression.BindProperty(property));
//                                    indexMap[property] = selectorExpressions.Count - 1;
//                                }

//                                _clientProjections[i] = Constant(indexMap);
//                                break;
//                            }

//                        case FileStoreQueryExpression fileStoreQueryExpression:
//                            {
//                                bool singleResult = fileStoreQueryExpression._scalarServerQuery
//                                                    || fileStoreQueryExpression._singleResultMethodInfo != null;
//                                fileStoreQueryExpression.ApplyProjection();
//                                Expression serverQuery = fileStoreQueryExpression.ServerQueryExpression;
//                                if (singleResult)
//                                {
//                                    serverQuery = ((LambdaExpression)((NewExpression)serverQuery).Arguments[0]).Body;
//                                }

//                                selectorExpressions.Add(serverQuery);
//                                _clientProjections[i] = Constant(selectorExpressions.Count - 1);
//                                break;
//                            }

//                        default:
//                            selectorExpressions.Add(projection);
//                            _clientProjections[i] = Constant(selectorExpressions.Count - 1);
//                            break;
//                    }
//                }
//            }
//            else
//            {
//                Dictionary<ProjectionMember, Expression> newProjectionMapping = new Dictionary<ProjectionMember, Expression>();
//                foreach ((ProjectionMember projectionMember, Expression expression) in _projectionMapping)
//                {
//                    if (expression is EntityProjectionExpression entityProjectionExpression)
//                    {
//                        Dictionary<IProperty, int> indexMap = new Dictionary<IProperty, int>();
//                        foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//                        {
//                            selectorExpressions.Add(entityProjectionExpression.BindProperty(property));
//                            indexMap[property] = selectorExpressions.Count - 1;
//                        }

//                        newProjectionMapping[projectionMember] = Constant(indexMap);
//                    }
//                    else
//                    {
//                        selectorExpressions.Add(expression);
//                        newProjectionMapping[projectionMember] = Constant(selectorExpressions.Count - 1);
//                    }
//                }

//                _projectionMapping = newProjectionMapping;
//                _projectionMappingExpressions.Clear();
//            }

//            LambdaExpression selectorLambda = Lambda(
//                New(
//                    ValueBufferConstructor,
//                    NewArrayInit(
//                        typeof(object),
//                        selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
//                CurrentParameter);

//            ServerQueryExpression = Call(
//                EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
//                ServerQueryExpression,
//                selectorLambda);

//            _groupingParameter = null;

//            if (_singleResultMethodInfo != null)
//            {
//                ServerQueryExpression = Call(
//                    _singleResultMethodInfo.MakeGenericMethod(CurrentParameter.Type),
//                    ServerQueryExpression);

//                ConvertToEnumerable();

//                _singleResultMethodInfo = null;
//            }
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void UpdateServerQueryExpression(Expression serverQueryExpression)
//            => ServerQueryExpression = serverQueryExpression;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ApplySetOperation(MethodInfo setOperationMethodInfo, FileStoreQueryExpression source2)
//        {
//            //Check.DebugAssert(_groupingParameter == null, "Cannot apply set operation after GroupBy without flattening.");
//            if (_clientProjections.Count == 0)
//            {
//                Dictionary<ProjectionMember, Expression> projectionMapping = new Dictionary<ProjectionMember, Expression>();
//                List<Expression> source1SelectorExpressions = new List<Expression>();
//                List<Expression> source2SelectorExpressions = new List<Expression>();
//                foreach ((ProjectionMember key, Expression value1, Expression value2) in _projectionMapping.Join(
//                             source2._projectionMapping, kv => kv.Key, kv => kv.Key,
//                             (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
//                {
//                    if (value1 is EntityProjectionExpression entityProjection1
//                        && value2 is EntityProjectionExpression entityProjection2)
//                    {
//                        Dictionary<IProperty, MethodCallExpression> map = new Dictionary<IProperty, MethodCallExpression>();
//                        foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjection1.EntityType))
//                        {
//                            ColumnExpression expressionToAdd1 = entityProjection1.BindProperty(property);
//                            ColumnExpression expressionToAdd2 = entityProjection2.BindProperty(property);
//                            source1SelectorExpressions.Add(expressionToAdd1);
//                            source2SelectorExpressions.Add(expressionToAdd2);
//                            Type type = expressionToAdd1.Type;
//                            if (!type.IsNullableType()
//                                && expressionToAdd2.Type.IsNullableType())
//                            {
//                                type = expressionToAdd2.Type;
//                            }

//                            map[property] = CreateReadValueExpression(type, source1SelectorExpressions.Count - 1, property);
//                        }

//                        projectionMapping[key] = new EntityProjectionExpression(entityProjection1.EntityType, map);
//                    }
//                    else
//                    {
//                        source1SelectorExpressions.Add(value1);
//                        source2SelectorExpressions.Add(value2);
//                        Type type = value1.Type;
//                        if (!type.IsNullableType()
//                            && value2.Type.IsNullableType())
//                        {
//                            type = value2.Type;
//                        }

//                        projectionMapping[key] = CreateReadValueExpression(
//                            type, source1SelectorExpressions.Count - 1, InferPropertyFromInner(value1));
//                    }
//                }

//                _projectionMapping = projectionMapping;

//                ServerQueryExpression = Call(
//                    EnumerableMethods.Select.MakeGenericMethod(ServerQueryExpression.Type.GetSequenceType(), typeof(ValueBuffer)),
//                    ServerQueryExpression,
//                    Lambda(
//                        New(
//                            ValueBufferConstructor,
//                            NewArrayInit(
//                                typeof(object),
//                                source1SelectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                        CurrentParameter));

//                source2.ServerQueryExpression = Call(
//                    EnumerableMethods.Select.MakeGenericMethod(source2.ServerQueryExpression.Type.GetSequenceType(), typeof(ValueBuffer)),
//                    source2.ServerQueryExpression,
//                    Lambda(
//                        New(
//                            ValueBufferConstructor,
//                            NewArrayInit(
//                                typeof(object),
//                                source2SelectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                        source2.CurrentParameter));
//            }
//            else
//            {
//                throw new InvalidOperationException("SetOperationsNotAllowedAfterClientEvaluation");
//            }

//            ServerQueryExpression = Call(
//                setOperationMethodInfo.MakeGenericMethod(typeof(ValueBuffer)), ServerQueryExpression, source2.ServerQueryExpression);
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ApplyDefaultIfEmpty()
//        {
//            if (_clientProjections.Count != 0)
//            {
//                throw new InvalidOperationException("InMemoryStrings.DefaultIfEmptyAppliedAfterProjection");
//            }

//            Dictionary<ProjectionMember, Expression> projectionMapping = new Dictionary<ProjectionMember, Expression>();
//            foreach ((ProjectionMember projectionMember, Expression expression) in _projectionMapping)
//            {
//                projectionMapping[projectionMember] = expression is EntityProjectionExpression entityProjectionExpression
//                    ? MakeEntityProjectionNullable(entityProjectionExpression)
//                    : MakeReadValueNullable(expression);
//            }

//            _projectionMapping = projectionMapping;
//            List<MethodCallExpression> projectionMappingExpressions = _projectionMappingExpressions.Select(e => MakeReadValueNullable(e)).ToList();
//            _projectionMappingExpressions.Clear();
//            _projectionMappingExpressions.AddRange(projectionMappingExpressions);
//            _groupingParameter = null;

//            ServerQueryExpression = Call(
//                EnumerableMethods.DefaultIfEmptyWithArgument.MakeGenericMethod(typeof(ValueBuffer)),
//                ServerQueryExpression,
//                Constant(new ValueBuffer(Enumerable.Repeat((object?)null, _projectionMappingExpressions.Count).ToArray())));
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ApplyDistinct()
//        {
//            //Check.DebugAssert(!_scalarServerQuery && _singleResultMethodInfo == null, "Cannot apply distinct on single result query");
//            //Check.DebugAssert(_groupingParameter == null, "Cannot apply distinct after GroupBy before flattening.");

//            List<Expression> selectorExpressions = new List<Expression>();
//            if (_clientProjections.Count == 0)
//            {
//                selectorExpressions.AddRange(_projectionMappingExpressions);
//                if (selectorExpressions.Count == 0)
//                {
//                    // No server correlated term in projection so add dummy 1.
//                    selectorExpressions.Add(Constant(1));
//                }
//            }
//            else
//            {
//                for (int i = 0; i < _clientProjections.Count; i++)
//                {
//                    Expression projection = _clientProjections[i];
//                    if (projection is FileStoreQueryExpression)
//                    {
//                        throw new InvalidOperationException("InMemoryStrings.DistinctOnSubqueryNotSupported");
//                    }

//                    if (projection is EntityProjectionExpression entityProjectionExpression)
//                    {
//                        _clientProjections[i] = TraverseEntityProjection(
//                            selectorExpressions, entityProjectionExpression, makeNullable: false);
//                    }
//                    else
//                    {
//                        selectorExpressions.Add(projection);
//                        _clientProjections[i] = CreateReadValueExpression(
//                            projection.Type, selectorExpressions.Count - 1, InferPropertyFromInner(projection));
//                    }
//                }
//            }

//            LambdaExpression selectorLambda = Lambda(
//                New(
//                    ValueBufferConstructor,
//                    NewArrayInit(
//                        typeof(object),
//                        selectorExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e).ToArray())),
//                CurrentParameter);

//            ServerQueryExpression = Call(
//                EnumerableMethods.Distinct.MakeGenericMethod(typeof(ValueBuffer)),
//                Call(
//                    EnumerableMethods.Select.MakeGenericMethod(CurrentParameter.Type, typeof(ValueBuffer)),
//                    ServerQueryExpression,
//                    selectorLambda));
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual GroupByShaperExpression ApplyGrouping(
//            Expression groupingKey,
//            Expression shaperExpression,
//            bool defaultElementSelector)
//        {
//            Expression source = ServerQueryExpression;
//            Expression? selector;
//            if (defaultElementSelector)
//            {
//                selector = Lambda(
//                    New(
//                        ValueBufferConstructor,
//                        NewArrayInit(
//                            typeof(object),
//                            _projectionMappingExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                    _valueBufferParameter);
//            }
//            else
//            {
//                MethodCallExpression selectMethodCall = (MethodCallExpression)ServerQueryExpression;
//                source = selectMethodCall.Arguments[0];
//                selector = selectMethodCall.Arguments[1];
//            }

//            _groupingParameter = Parameter(typeof(IGrouping<ValueBuffer, ValueBuffer>), "grouping");
//            MemberExpression groupingKeyAccessExpression = PropertyOrField(_groupingParameter, nameof(IGrouping<int, int>.Key));
//            List<Expression> groupingKeyExpressions = new List<Expression>();
//            groupingKey = GetGroupingKey(groupingKey, groupingKeyExpressions, groupingKeyAccessExpression);
//            LambdaExpression keySelector = Lambda(
//                New(
//                    ValueBufferConstructor,
//                    NewArrayInit(
//                        typeof(object),
//                        groupingKeyExpressions.Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                _valueBufferParameter);

//            ServerQueryExpression = Call(
//                EnumerableMethods.GroupByWithKeyElementSelector.MakeGenericMethod(
//                    typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
//                source,
//                keySelector,
//                selector);

//            var clonedInMemoryQueryExpression = Clone();
//            clonedInMemoryQueryExpression.UpdateServerQueryExpression(_groupingParameter);
//            clonedInMemoryQueryExpression._groupingParameter = null;

//            return new GroupByShaperExpression(
//                groupingKey,
//                new ShapedQueryExpression(
//                    clonedInMemoryQueryExpression,
//                    new QueryExpressionReplacingExpressionVisitor(this, clonedInMemoryQueryExpression).Visit(shaperExpression)));
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression AddInnerJoin(
//            FileStoreQueryExpression innerQueryExpression,
//            LambdaExpression outerKeySelector,
//            LambdaExpression innerKeySelector,
//            Expression outerShaperExpression,
//            Expression innerShaperExpression)
//            => AddJoin(
//                innerQueryExpression, outerKeySelector, innerKeySelector, outerShaperExpression, innerShaperExpression,
//                innerNullable: false);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression AddLeftJoin(
//            FileStoreQueryExpression innerQueryExpression,
//            LambdaExpression outerKeySelector,
//            LambdaExpression innerKeySelector,
//            Expression outerShaperExpression,
//            Expression innerShaperExpression)
//            => AddJoin(
//                innerQueryExpression, outerKeySelector, innerKeySelector, outerShaperExpression, innerShaperExpression,
//                innerNullable: true);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression AddSelectMany(
//            FileStoreQueryExpression innerQueryExpression,
//            Expression outerShaperExpression,
//            Expression innerShaperExpression,
//            bool innerNullable)
//            => AddJoin(innerQueryExpression, null, null, outerShaperExpression, innerShaperExpression, innerNullable);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual EntityShaperExpression AddNavigationToWeakEntityType(
//            EntityProjectionExpression entityProjectionExpression,
//            INavigation navigation,
//            FileStoreQueryExpression innerQueryExpression,
//            LambdaExpression outerKeySelector,
//            LambdaExpression innerKeySelector)
//        {
//            //Check.DebugAssert(_clientProjections.Count == 0, "Cannot expand weak entity navigation after client projection yet.");
//            ParameterExpression outerParameter = Parameter(typeof(ValueBuffer), "outer");
//            ParameterExpression innerParameter = Parameter(typeof(ValueBuffer), "inner");
//            ReplacingExpressionVisitor replacingVisitor = new ReplacingExpressionVisitor(
//                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
//                new Expression[] { outerParameter, innerParameter });

//            List<Expression> selectorExpressions = _projectionMappingExpressions.Select(e => replacingVisitor.Visit(e)).ToList();
//            int outerIndex = selectorExpressions.Count;
//            EntityProjectionExpression innerEntityProjection = (EntityProjectionExpression)innerQueryExpression._projectionMapping[new ProjectionMember()];
//            Dictionary<IProperty, MethodCallExpression> innerReadExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
//            foreach (IProperty property in GetAllPropertiesInHierarchy(innerEntityProjection.EntityType))
//            {
//                ColumnExpression propertyExpression = innerEntityProjection.BindProperty(property);
//                propertyExpression = MakeReadValueNullable(propertyExpression);

//                selectorExpressions.Add(propertyExpression);
//                MethodCallExpression readValueExpression = CreateReadValueExpression(propertyExpression.Type, selectorExpressions.Count - 1, property);
//                innerReadExpressionMap[property] = readValueExpression;
//                _projectionMappingExpressions.Add(readValueExpression);
//            }

//            innerEntityProjection = new EntityProjectionExpression(innerEntityProjection.EntityType, innerReadExpressionMap);

//            LambdaExpression resultSelector = Lambda(
//                New(
//                    ValueBufferConstructor,
//                    NewArrayInit(
//                        typeof(object),
//                        selectorExpressions
//                            .Select(e => replacingVisitor.Visit(e))
//                            .Select(e => e.Type.IsValueType ? Convert(e, typeof(object)) : e))),
//                outerParameter,
//                innerParameter);

//            ServerQueryExpression = Call(
//                LeftJoinMethodInfo.MakeGenericMethod(
//                    typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
//                ServerQueryExpression,
//                innerQueryExpression.ServerQueryExpression,
//                outerKeySelector,
//                innerKeySelector,
//                resultSelector,
//                Constant(
//                    new ValueBuffer(
//                        Enumerable.Repeat((object?)null, selectorExpressions.Count - outerIndex).ToArray())));

//            EntityShaperExpression entityShaper = new EntityShaperExpression(innerEntityProjection.EntityType, innerEntityProjection, nullable: true);
//            entityProjectionExpression.AddNavigationBinding(navigation, entityShaper);

//            return entityShaper;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual ShapedQueryExpression Clone(Expression shaperExpression)
//        {
//            var clonedInMemoryQueryExpression = Clone();

//            return new ShapedQueryExpression(
//                clonedInMemoryQueryExpression,
//                new QueryExpressionReplacingExpressionVisitor(this, clonedInMemoryQueryExpression).Visit(shaperExpression));
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual Expression GetSingleScalarProjection()
//        {
//            MethodCallExpression expression = CreateReadValueExpression(ServerQueryExpression.Type, 0, null);
//            _projectionMapping.Clear();
//            _projectionMappingExpressions.Clear();
//            _clientProjections.Clear();
//            _projectionMapping[new ProjectionMember()] = expression;
//            _projectionMappingExpressions.Add(expression);
//            _groupingParameter = null;

//            _scalarServerQuery = true;
//            ConvertToEnumerable();

//            return new ProjectionBindingExpression(this, new ProjectionMember(), expression.Type.MakeNullable());
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public virtual void ConvertToSingleResult(MethodInfo methodInfo)
//            => _singleResultMethodInfo = methodInfo;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public override Type Type
//            => typeof(IEnumerable<ValueBuffer>);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public sealed override ExpressionType NodeType
//            => ExpressionType.Extension;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
//        {
//            expressionPrinter.AppendLine(nameof(InMemoryQueryExpression) + ": ");
//            using (expressionPrinter.Indent())
//            {
//                expressionPrinter.AppendLine(nameof(ServerQueryExpression) + ": ");
//                using (expressionPrinter.Indent())
//                {
//                    expressionPrinter.Visit(ServerQueryExpression);
//                }

//                expressionPrinter.AppendLine();
//                if (_clientProjections.Count > 0)
//                {
//                    expressionPrinter.AppendLine("ClientProjections:");
//                    using (expressionPrinter.Indent())
//                    {
//                        for (int i = 0; i < _clientProjections.Count; i++)
//                        {
//                            expressionPrinter.AppendLine();
//                            expressionPrinter.Append(i.ToString()).Append(" -> ");
//                            expressionPrinter.Visit(_clientProjections[i]);
//                        }
//                    }
//                }
//                else
//                {
//                    expressionPrinter.AppendLine("ProjectionMapping:");
//                    using (expressionPrinter.Indent())
//                    {
//                        foreach ((ProjectionMember projectionMember, Expression expression) in _projectionMapping)
//                        {
//                            expressionPrinter.Append("Member: " + projectionMember + " Projection: ");
//                            expressionPrinter.Visit(expression);
//                            expressionPrinter.AppendLine(",");
//                        }
//                    }
//                }

//                expressionPrinter.AppendLine();
//            }
//        }

//        private FileStoreQueryExpression Clone()
//        {
//            _cloningExpressionVisitor ??= new CloningExpressionVisitor();

//            return (FileStoreQueryExpression)_cloningExpressionVisitor.Visit(this);
//        }

//        private static Expression GetGroupingKey(Expression key, List<Expression> groupingExpressions, Expression groupingKeyAccessExpression)
//        {
//            switch (key)
//            {
//                case NewExpression newExpression:
//                    Expression[] arguments = new Expression[newExpression.Arguments.Count];
//                    for (int i = 0; i < arguments.Length; i++)
//                    {
//                        arguments[i] = GetGroupingKey(newExpression.Arguments[i], groupingExpressions, groupingKeyAccessExpression);
//                    }

//                    return newExpression.Update(arguments);

//                case MemberInitExpression memberInitExpression:
//                    if (memberInitExpression.Bindings.Any(mb => !(mb is MemberAssignment)))
//                    {
//                        goto default;
//                    }

//                    NewExpression updatedNewExpression = (NewExpression)GetGroupingKey(
//                        memberInitExpression.NewExpression, groupingExpressions, groupingKeyAccessExpression);
//                    MemberAssignment[] memberBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
//                    for (int i = 0; i < memberBindings.Length; i++)
//                    {
//                        MemberAssignment memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
//                        memberBindings[i] = memberAssignment.Update(
//                            GetGroupingKey(
//                                memberAssignment.Expression,
//                                groupingExpressions,
//                                groupingKeyAccessExpression));
//                    }

//                    return memberInitExpression.Update(updatedNewExpression, memberBindings);

//                case EntityShaperExpression entityShaperExpression
//                    when entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression:
//                    EntityProjectionExpression entityProjectionExpression = (EntityProjectionExpression)((FileStoreQueryExpression)projectionBindingExpression.QueryExpression)
//                        .GetProjection(projectionBindingExpression);
//                    Dictionary<IProperty, MethodCallExpression> readExpressions = new Dictionary<IProperty, MethodCallExpression>();
//                    foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//                    {
//                        readExpressions[property] = (MethodCallExpression)GetGroupingKey(
//                            entityProjectionExpression.BindProperty(property),
//                            groupingExpressions,
//                            groupingKeyAccessExpression);
//                    }

//                    return entityShaperExpression.Update(
//                        new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressions));

//                default:
//                    int index = groupingExpressions.Count;
//                    groupingExpressions.Add(key);
//                    return groupingKeyAccessExpression.CreateValueBufferReadValueExpression(
//                        key.Type,
//                        index,
//                        InferPropertyFromInner(key));
//            }
//        }

//        private Expression AddJoin(
//            FileStoreQueryExpression innerQueryExpression,
//            LambdaExpression? outerKeySelector,
//            LambdaExpression? innerKeySelector,
//            Expression outerShaperExpression,
//            Expression innerShaperExpression,
//            bool innerNullable)
//        {
//            Type transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaperExpression.Type, innerShaperExpression.Type);
//            FieldInfo outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer")!;
//            FieldInfo innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner")!;
//            bool outerClientEval = _clientProjections.Count > 0;
//            bool innerClientEval = innerQueryExpression._clientProjections.Count > 0;
//            List<Expression> resultSelectorExpressions = new List<Expression>();
//            ParameterExpression outerParameter = Parameter(typeof(ValueBuffer), "outer");
//            ParameterExpression innerParameter = Parameter(typeof(ValueBuffer), "inner");
//            ReplacingExpressionVisitor replacingVisitor = new ReplacingExpressionVisitor(
//                new Expression[] { CurrentParameter, innerQueryExpression.CurrentParameter },
//                new Expression[] { outerParameter, innerParameter });
//            int outerIndex;

//            if (outerClientEval)
//            {
//                // Outer projection are already populated
//                if (innerClientEval)
//                {
//                    // Add inner to projection and update indexes
//                    int[] indexMap = new int[innerQueryExpression._clientProjections.Count];
//                    for (int i = 0; i < innerQueryExpression._clientProjections.Count; i++)
//                    {
//                        Expression projectionToAdd = innerQueryExpression._clientProjections[i];
//                        projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
//                        _clientProjections.Add(projectionToAdd);
//                        indexMap[i] = _clientProjections.Count - 1;
//                    }

//                    innerQueryExpression._clientProjections.Clear();

//                    innerShaperExpression =
//                        new ProjectionIndexRemappingExpressionVisitor(innerQueryExpression, this, indexMap).Visit(innerShaperExpression);
//                }
//                else
//                {
//                    // Apply inner projection mapping and convert projection member binding to indexes
//                    Dictionary<ProjectionMember, int> mapping = ConvertProjectionMappingToClientProjections(innerQueryExpression._projectionMapping, innerNullable);
//                    innerShaperExpression =
//                        new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(innerShaperExpression);
//                }

//                // TODO: We still need to populate and generate result selector
//                // Further for a subquery in projection we may need to update correlation terms used inside it.
//                throw new NotImplementedException();
//            }

//            if (innerClientEval)
//            {
//                // Since inner projections are populated, we need to populate outer also
//                Dictionary<ProjectionMember, int> mapping = ConvertProjectionMappingToClientProjections(_projectionMapping);
//                outerShaperExpression = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(outerShaperExpression);

//                int[] indexMap = new int[innerQueryExpression._clientProjections.Count];
//                for (int i = 0; i < innerQueryExpression._clientProjections.Count; i++)
//                {
//                    Expression projectionToAdd = innerQueryExpression._clientProjections[i];
//                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
//                    _clientProjections.Add(projectionToAdd);
//                    indexMap[i] = _clientProjections.Count - 1;
//                }

//                innerQueryExpression._clientProjections.Clear();

//                innerShaperExpression =
//                    new ProjectionIndexRemappingExpressionVisitor(innerQueryExpression, this, indexMap).Visit(innerShaperExpression);
//                // TODO: We still need to populate and generate result selector
//                // Further for a subquery in projection we may need to update correlation terms used inside it.
//                throw new NotImplementedException();
//            }
//            else
//            {
//                Dictionary<ProjectionMember, Expression> projectionMapping = new Dictionary<ProjectionMember, Expression>();
//                Dictionary<ProjectionMember, ProjectionMember> mapping = new Dictionary<ProjectionMember, ProjectionMember>();
//                foreach ((ProjectionMember projectionMember, Expression expression) in _projectionMapping)
//                {
//                    ProjectionMember newProjectionMember = projectionMember.Prepend(outerMemberInfo);
//                    mapping[projectionMember] = newProjectionMember;
//                    if (expression is EntityProjectionExpression entityProjectionExpression)
//                    {
//                        projectionMapping[newProjectionMember] = TraverseEntityProjection(
//                            resultSelectorExpressions, entityProjectionExpression, makeNullable: false);
//                    }
//                    else
//                    {
//                        resultSelectorExpressions.Add(expression);
//                        projectionMapping[newProjectionMember] = CreateReadValueExpression(
//                            expression.Type, resultSelectorExpressions.Count - 1, InferPropertyFromInner(expression));
//                    }
//                }

//                outerShaperExpression = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(outerShaperExpression);
//                mapping.Clear();

//                outerIndex = resultSelectorExpressions.Count;
//                foreach (KeyValuePair<ProjectionMember, Expression> projection in innerQueryExpression._projectionMapping)
//                {
//                    ProjectionMember newProjectionMember = projection.Key.Prepend(innerMemberInfo);
//                    mapping[projection.Key] = newProjectionMember;
//                    if (projection.Value is EntityProjectionExpression entityProjectionExpression)
//                    {
//                        projectionMapping[newProjectionMember] = TraverseEntityProjection(
//                            resultSelectorExpressions, entityProjectionExpression, innerNullable);
//                    }
//                    else
//                    {
//                        Expression expression = projection.Value;
//                        if (innerNullable)
//                        {
//                            expression = MakeReadValueNullable(expression);
//                        }

//                        resultSelectorExpressions.Add(expression);
//                        projectionMapping[newProjectionMember] = CreateReadValueExpression(
//                            expression.Type, resultSelectorExpressions.Count - 1, InferPropertyFromInner(projection.Value));
//                    }
//                }

//                innerShaperExpression = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(innerShaperExpression);
//                mapping.Clear();

//                _projectionMapping = projectionMapping;
//            }

//            LambdaExpression resultSelector = Lambda(
//                New(
//                    ValueBufferConstructor, NewArrayInit(
//                        typeof(object),
//                        resultSelectorExpressions.Select(
//                            (e, i) =>
//                            {
//                                Expression expression = replacingVisitor.Visit(e);
//                                if (innerNullable
//                                    && i > outerIndex)
//                                {
//                                    expression = MakeReadValueNullable(expression);
//                                }

//                                if (expression.Type.IsValueType)
//                                {
//                                    expression = Convert(expression, typeof(object));
//                                }

//                                return expression;
//                            }))),
//                outerParameter,
//                innerParameter);

//            if (outerKeySelector != null
//                && innerKeySelector != null)
//            {
//                if (innerNullable)
//                {
//                    ServerQueryExpression = Call(
//                        LeftJoinMethodInfo.MakeGenericMethod(
//                            typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
//                        ServerQueryExpression,
//                        innerQueryExpression.ServerQueryExpression,
//                        outerKeySelector,
//                        innerKeySelector,
//                        resultSelector,
//                        Constant(
//                            new ValueBuffer(
//                                Enumerable.Repeat((object?)null, resultSelectorExpressions.Count - outerIndex).ToArray())));
//                }
//                else
//                {
//                    ServerQueryExpression = Call(
//                        EnumerableMethods.Join.MakeGenericMethod(
//                            typeof(ValueBuffer), typeof(ValueBuffer), outerKeySelector.ReturnType, typeof(ValueBuffer)),
//                        ServerQueryExpression,
//                        innerQueryExpression.ServerQueryExpression,
//                        outerKeySelector,
//                        innerKeySelector,
//                        resultSelector);
//                }
//            }
//            else
//            {
//                // inner nullable should do something different here
//                // Issue#17536
//                ServerQueryExpression = Call(
//                    EnumerableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
//                        typeof(ValueBuffer), typeof(ValueBuffer), typeof(ValueBuffer)),
//                    ServerQueryExpression,
//                    Lambda(innerQueryExpression.ServerQueryExpression, CurrentParameter),
//                    resultSelector);
//            }

//            if (innerNullable)
//            {
//                innerShaperExpression = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaperExpression);
//            }

//            return New(
//                transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
//                new[] { outerShaperExpression, innerShaperExpression }, outerMemberInfo, innerMemberInfo);

//            static Expression MakeNullable(Expression expression, bool nullable)
//                => nullable
//                    ? expression is EntityProjectionExpression entityProjection
//                        ? MakeEntityProjectionNullable(entityProjection)
//                        : MakeReadValueNullable(expression)
//                    : expression;
//        }

//        private void ConvertToEnumerable()
//        {
//            if (_scalarServerQuery || _singleResultMethodInfo != null)
//            {
//                if (ServerQueryExpression.Type != typeof(ValueBuffer))
//                {
//                    if (ServerQueryExpression.Type.IsValueType)
//                    {
//                        ServerQueryExpression = Convert(ServerQueryExpression, typeof(object));
//                    }

//                    ServerQueryExpression = New(
//                        ResultEnumerableConstructor,
//                        Lambda<Func<ValueBuffer>>(
//                            New(
//                                ValueBufferConstructor,
//                                NewArrayInit(typeof(object), ServerQueryExpression))));
//                }
//                else
//                {
//                    ServerQueryExpression = New(
//                        ResultEnumerableConstructor,
//                        Lambda<Func<ValueBuffer>>(ServerQueryExpression));
//                }
//            }
//        }

//        private MethodCallExpression CreateReadValueExpression(Type type, int index, IPropertyBase? property)
//            => (MethodCallExpression)_valueBufferParameter.CreateValueBufferReadValueExpression(type, index, property);

//        private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
//            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
//                .SelectMany(t => t.GetDeclaredProperties());

//        private static IPropertyBase? InferPropertyFromInner(Expression expression)
//            => expression is MethodCallExpression methodCallExpression
//                && methodCallExpression.Method.IsGenericMethod
//                && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod
//                    ? methodCallExpression.Arguments[2].GetConstantValue<IPropertyBase>()
//                    : null;

//        private static EntityProjectionExpression MakeEntityProjectionNullable(EntityProjectionExpression entityProjectionExpression)
//        {
//            Dictionary<IProperty, MethodCallExpression> readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
//            foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//            {
//                readExpressionMap[property] = MakeReadValueNullable(entityProjectionExpression.BindProperty(property));
//            }

//            EntityProjectionExpression result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

//            // Also compute nested entity projections
//            foreach (INavigation navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
//                         .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
//                         .SelectMany(t => t.GetDeclaredNavigations()))
//            {
//                EntityShaperExpression boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
//                if (boundEntityShaperExpression != null)
//                {
//                    EntityProjectionExpression innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
//                    EntityProjectionExpression newInnerEntityProjection = MakeEntityProjectionNullable(innerEntityProjection);
//                    boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
//                    result.AddNavigationBinding(navigation, boundEntityShaperExpression);
//                }
//            }

//            return result;
//        }

//        private Dictionary<ProjectionMember, int> ConvertProjectionMappingToClientProjections(
//            Dictionary<ProjectionMember, Expression> projectionMapping,
//            bool makeNullable = false)
//        {
//            Dictionary<ProjectionMember, int> mapping = new Dictionary<ProjectionMember, int>();
//            Dictionary<EntityProjectionExpression, int> entityProjectionCache = new Dictionary<EntityProjectionExpression, int>(ReferenceEqualityComparer.Instance);
//            foreach (KeyValuePair<ProjectionMember, Expression> projection in projectionMapping)
//            {
//                ProjectionMember projectionMember = projection.Key;
//                Expression projectionToAdd = projection.Value;

//                if (projectionToAdd is EntityProjectionExpression entityProjection)
//                {
//                    if (!entityProjectionCache.TryGetValue(entityProjection, out int value))
//                    {
//                        EntityProjectionExpression entityProjectionToCache = entityProjection;
//                        if (makeNullable)
//                        {
//                            entityProjection = MakeEntityProjectionNullable(entityProjection);
//                        }

//                        _clientProjections.Add(entityProjection);
//                        value = _clientProjections.Count - 1;
//                        entityProjectionCache[entityProjectionToCache] = value;
//                    }

//                    mapping[projectionMember] = value;
//                }
//                else
//                {
//                    if (makeNullable)
//                    {
//                        projectionToAdd = MakeReadValueNullable(projectionToAdd);
//                    }

//                    int existingIndex = _clientProjections.FindIndex(e => e.Equals(projectionToAdd));
//                    if (existingIndex == -1)
//                    {
//                        _clientProjections.Add(projectionToAdd);
//                        existingIndex = _clientProjections.Count - 1;
//                    }

//                    mapping[projectionMember] = existingIndex;
//                }
//            }

//            projectionMapping.Clear();

//            return mapping;
//        }

//        private static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
//            IEnumerable<TOuter> outer,
//            IEnumerable<TInner> inner,
//            Func<TOuter, TKey> outerKeySelector,
//            Func<TInner, TKey> innerKeySelector,
//            Func<TOuter, TInner, TResult> resultSelector,
//            TInner defaultValue)
//            => outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (oe, ies) => new { oe, ies })
//                .SelectMany(t => t.ies.DefaultIfEmpty(defaultValue), (t, i) => resultSelector(t.oe, i));

//        private static MethodCallExpression MakeReadValueNullable(Expression expression)
//        {
//            //Check.DebugAssert(expression is MethodCallExpression, "Expression must be method call expression.");

//            MethodCallExpression methodCallExpression = (MethodCallExpression)expression;

//            return methodCallExpression.Type.IsNullableType()
//                ? methodCallExpression
//                : Call(
//                    ExpressionExtensions.ValueBufferTryReadValueMethod.MakeGenericMethod(methodCallExpression.Type.MakeNullable()),
//                    methodCallExpression.Arguments);
//        }

//        private EntityProjectionExpression TraverseEntityProjection(
//            List<Expression> selectorExpressions,
//            EntityProjectionExpression entityProjectionExpression,
//            bool makeNullable)
//        {
//            Dictionary<IProperty, MethodCallExpression> readExpressionMap = new Dictionary<IProperty, MethodCallExpression>();
//            foreach (IProperty property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
//            {
//                ColumnExpression expression = entityProjectionExpression.BindProperty(property);
//                if (makeNullable)
//                {
//                    expression = MakeReadValueNullable(expression);
//                }

//                selectorExpressions.Add(expression);
//                MethodCallExpression newExpression = CreateReadValueExpression(expression.Type, selectorExpressions.Count - 1, property);
//                readExpressionMap[property] = newExpression;
//            }

//            EntityProjectionExpression result = new EntityProjectionExpression(entityProjectionExpression.EntityType, readExpressionMap);

//            // Also compute nested entity projections
//            foreach (INavigation navigation in entityProjectionExpression.EntityType.GetAllBaseTypes()
//                         .Concat(entityProjectionExpression.EntityType.GetDerivedTypesInclusive())
//                         .SelectMany(t => t.GetDeclaredNavigations()))
//            {
//                EntityShaperExpression boundEntityShaperExpression = entityProjectionExpression.BindNavigation(navigation);
//                if (boundEntityShaperExpression != null)
//                {
//                    EntityProjectionExpression innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
//                    EntityProjectionExpression newInnerEntityProjection = TraverseEntityProjection(selectorExpressions, innerEntityProjection, makeNullable);
//                    boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
//                    result.AddNavigationBinding(navigation, boundEntityShaperExpression);
//                }
//            }

//            return result;
//        }
//    }