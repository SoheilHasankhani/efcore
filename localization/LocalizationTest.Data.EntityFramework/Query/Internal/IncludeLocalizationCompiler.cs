namespace LocalizationTest.Data.EntityFramework.Query.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using LocalizationTest.Data.EntityFramework.Entities.Basic;
    using LocalizationTest.Data.EntityFramework.Metadata.Internal;
    using LocalizationTest.Data.EntityFramework.Query.ResultOperators.Internal;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
    using Microsoft.EntityFrameworkCore.Extensions.Internal;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;

    using Remotion.Linq;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Clauses.Expressions;
    using Remotion.Linq.Clauses.ResultOperators;
    using Remotion.Linq.Clauses.StreamedData;
    using Remotion.Linq.Parsing;

    public class IncludeLocalizationCompiler
    {
        private static readonly MethodInfo _referenceEqualsMethodInfo
            = typeof(object).GetTypeInfo()
                .GetDeclaredMethod(nameof(ReferenceEquals));

        private static readonly MethodInfo _queryBufferStartTrackingMethodInfo
            = typeof(IQueryBuffer).GetTypeInfo()
                .GetDeclaredMethods(nameof(IQueryBuffer.StartTracking))
                .Single(mi => mi.GetParameters()[1].ParameterType == typeof(IEntityType));

        private static readonly MethodInfo _setRelationshipSnapshotValueMethodInfo
            = typeof(IncludeLocalizationCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(SetRelationshipSnapshotValue));

        private static void SetRelationshipSnapshotValue(
            IStateManager stateManager,
            IPropertyBase navigation,
            object entity,
            object value)
        {
            var internalEntityEntry = stateManager.TryGetEntry(entity);

            Debug.Assert(internalEntityEntry != null);

            internalEntityEntry.SetRelationshipSnapshotValue(navigation, value);
        }

        private static readonly MethodInfo _setRelationshipIsLoadedMethodInfo
            = typeof(IncludeLocalizationCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(SetRelationshipIsLoaded));

        private static void SetRelationshipIsLoaded(
            IStateManager stateManager,
            IPropertyBase navigation,
            object entity)
        {
            var internalEntityEntry = stateManager.TryGetEntry(entity);

            Debug.Assert(internalEntityEntry != null);

            internalEntityEntry.SetIsLoaded((INavigation)navigation);
        }

        private static readonly MethodInfo _includeLocalizationAsyncMethodInfo
            = typeof(IncludeLocalizationCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(_IncludeLocalizationAsync));

        private static readonly MethodInfo _includeLocalizationMethodInfo
            = typeof(IncludeLocalizationCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(_IncludeLocalization));

        private static readonly ParameterExpression _includedParameter = Expression.Parameter(typeof(object[]), name: "included");

        public static readonly ParameterExpression CancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), name: "ct");

        private readonly List<IncludeLocalizationResultOperator> _includeLocalizationResultOperators;
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;

        private QueryModel _targetQueryModel;
        private int _collectionIncludeId;

        public IncludeLocalizationCompiler(QueryCompilationContext queryCompilationContext, IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
        {
            _queryCompilationContext = queryCompilationContext;
            _querySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;

            _includeLocalizationResultOperators
                = _queryCompilationContext.QueryAnnotations
                    .OfType<IncludeLocalizationResultOperator>()
                    .ToList();
        }

        public virtual void CompileIncludeLocalizations(
            QueryModel queryModel,
            bool trackingQuery,
            bool asyncQuery,
            bool shouldThrow)
        {
            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            _targetQueryModel = _targetQueryModel ?? queryModel;

            var includeLocalizationLoadTrees = CreateIncludeLocalizationLoadTrees(queryModel, shouldThrow);

            foreach (var includeLoadTree in includeLocalizationLoadTrees)
            {
                includeLoadTree.Compile(
                    _queryCompilationContext,
                    _targetQueryModel,
                    trackingQuery,
                    asyncQuery,
                    ref _collectionIncludeId);
            }
        }

        private IEnumerable<IncludeLocalizationLoadTree> CreateIncludeLocalizationLoadTrees(QueryModel queryModel, bool shouldThrow)
        {
            var querySourceTracingExpressionVisitor
                = _querySourceTracingExpressionVisitorFactory.Create();

            var includeLoadTrees = new List<IncludeLocalizationLoadTree>();

            foreach (var includeLocalizationResultOperator in _includeLocalizationResultOperators.ToArray())
            {
                var querySourceReferenceExpression
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.GetOutputExpression(),
                            includeLocalizationResultOperator.QuerySource);

                if (querySourceReferenceExpression == null)
                {
                    continue;
                }

                if (querySourceReferenceExpression.Type.IsGrouping()
                    && querySourceTracingExpressionVisitor.OriginGroupByQueryModel != null)
                {
                    querySourceReferenceExpression
                        = querySourceTracingExpressionVisitor
                            .FindResultQuerySourceReferenceExpression(
                                querySourceTracingExpressionVisitor.OriginGroupByQueryModel.GetOutputExpression(),
                                includeLocalizationResultOperator.QuerySource);

                    _targetQueryModel = querySourceTracingExpressionVisitor.OriginGroupByQueryModel;
                }

                if (querySourceReferenceExpression?.Type.IsGrouping() != false)
                {
                    continue;
                }

                var includeLoadTree
                    = includeLoadTrees
                        .SingleOrDefault(
                            t => ReferenceEquals(
                                t.QuerySourceReferenceExpression, querySourceReferenceExpression));

                if (includeLoadTree == null)
                {
                    includeLoadTrees.Add(includeLoadTree = new IncludeLocalizationLoadTree(querySourceReferenceExpression));
                }

                // this property is never a real navigation property
                // if (!TryPopulateIncludeLoadTree(includeLocalizationResultOperator, includeLoadTree, shouldThrow))
                // {
                //     includeLoadTrees.Remove(includeLoadTree);
                //     continue;
                // }

                // _queryCompilationContext.Logger.NavigationIncluded(includeResultOperator);

                _includeLocalizationResultOperators.Remove(includeLocalizationResultOperator);
            }

            return includeLoadTrees;
        }

        private bool TryPopulateIncludeLoadTree(
            IncludeLocalizationResultOperator includeLocalizationResultOperator,
            IncludeLocalizationLoadTree includeLoadTree,
            bool shouldThrow)
        {
            if (includeLocalizationResultOperator.NavigationPaths != null)
            {
                foreach (var navigationPath in includeLocalizationResultOperator.NavigationPaths)
                {
                    includeLoadTree.AddLoadPath(navigationPath);
                }

                return true;
            }

            IEntityType entityType = null;
            if (includeLocalizationResultOperator.PathFromQuerySource is QuerySourceReferenceExpression qsre)
            {
                entityType = _queryCompilationContext.FindEntityType(qsre.ReferencedQuerySource);
            }

            if (entityType == null)
            {
                entityType = _queryCompilationContext.Model.FindEntityType(includeLocalizationResultOperator.PathFromQuerySource.Type);

                if (entityType == null)
                {
                    var pathFromSource = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                        includeLocalizationResultOperator.PathFromQuerySource, _queryCompilationContext, out qsre);

                    if (pathFromSource.Count > 0
                        && pathFromSource[pathFromSource.Count - 1] is INavigation navigation)
                    {
                        entityType = navigation.GetTargetType();
                    }
                }
            }

            if (entityType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                            includeLocalizationResultOperator.ToString(),
                            includeLocalizationResultOperator.NavigationPropertyPaths.FirstOrDefault()));
                }

                return false;
            }

            return WalkNavigations(entityType, includeLocalizationResultOperator.NavigationPropertyPaths, includeLoadTree, shouldThrow);
        }

        private static bool WalkNavigations(
            IEntityType entityType,
            IReadOnlyList<string> navigationPropertyPaths,
            IncludeLocalizationLoadTree includeLoadTree,
            bool shouldThrow)
        {
            var longestMatchFound
                = WalkNavigationsInternal(
                    entityType,
                    navigationPropertyPaths,
                    includeLoadTree,
                    new Stack<INavigation>(),
                    (0, entityType));

            if (longestMatchFound.Depth < navigationPropertyPaths.Count)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeBadNavigation(
                            navigationPropertyPaths[longestMatchFound.Depth],
                            longestMatchFound.EntityType.DisplayName()));
                }

                return false;
            }

            return true;
        }

        private static (int Depth, IEntityType EntityType) WalkNavigationsInternal(
            IEntityType entityType,
            IReadOnlyList<string> navigationPropertyPaths,
            IncludeLocalizationLoadTree includeLoadTree,
            Stack<INavigation> stack,
            (int Depth, IEntityType EntityType) longestMatchFound)
        {
            var entityTypeBase = (entityType as TypeBase);
            // entityTypeBase.GetIgnoredMembers().Where()

            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                    .Where(n => navigationPropertyPaths.Count > stack.Count && n.Name == navigationPropertyPaths[stack.Count])
                    .ToList();

            if (outboundNavigations.Count == 0
                && stack.Count > 0)
            {
                includeLoadTree.AddLoadPath(stack.Reverse().ToArray());

                if (stack.Count > longestMatchFound.Depth)
                {
                    longestMatchFound = (stack.Count, entityType);
                }
            }
            else
            {
                foreach (var navigation in outboundNavigations)
                {
                    stack.Push(navigation);

                    longestMatchFound
                        = WalkNavigationsInternal(
                            navigation.GetTargetType(),
                            navigationPropertyPaths,
                            includeLoadTree,
                            stack,
                            longestMatchFound);

                    stack.Pop();
                }
            }

            return longestMatchFound;
        }

        private static TEntity _IncludeLocalization<TEntity>(
            QueryContext queryContext,
            TEntity entity,
            object[] included,
            Action<QueryContext, TEntity, object[]> fixup)
        {
            if (entity != null)
            {
                fixup(queryContext, entity, included);
            }

            return entity;
        }

        private static async Task<TEntity> _IncludeLocalizationAsync<TEntity>(
            QueryContext queryContext,
            TEntity entity,
            object[] included,
            Func<QueryContext, TEntity, object[], CancellationToken, Task> fixup,
            CancellationToken cancellationToken)
        {
            if (entity != null)
            {
                await fixup(queryContext, entity, included, cancellationToken);
            }

            return entity;
        }

        private abstract class IncludeLocalizationLoadTreeNodeBase
        {
            protected static void AddLoadPath(
                IncludeLocalizationLoadTreeNodeBase node,
                IReadOnlyList<INavigation> navigationPath,
                int index)
            {
                while (index < navigationPath.Count)
                {
                    var navigation = navigationPath[index];
                    var childNode = node.Children.SingleOrDefault(n => n.Navigation == navigation);

                    if (childNode == null)
                    {
                        node.Children.Add(childNode = new IncludeLocalizationLoadTreeNode(navigation));

                        var targetType = navigation.GetTargetType();

                        var outboundNavigations
                            = targetType.GetNavigations()
                                .Concat(targetType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                                .Where(n => n.IsEagerLoaded);

                        foreach (var outboundNavigation in outboundNavigations)
                        {
                            AddLoadPath(childNode, new[] { outboundNavigation }, index: 0);
                        }
                    }

                    node = childNode;
                    index = index + 1;
                }
            }

            protected ICollection<IncludeLocalizationLoadTreeNode> Children { get; } = new List<IncludeLocalizationLoadTreeNode>();

            protected void Compile(
                QueryCompilationContext queryCompilationContext,
                QueryModel queryModel,
                bool trackingQuery,
                bool asyncQuery,
                ref int collectionIncludeId,
                QuerySourceReferenceExpression targetQuerySourceReferenceExpression)
            {
                var entityParameter = Expression.Parameter(targetQuerySourceReferenceExpression.Type, name: "entity");

                var propertyExpressions = new List<Expression>();
                var blockExpressions = new List<Expression>();

                // var entityType
                //     = queryCompilationContext.FindEntityType(targetQuerySourceReferenceExpression.ReferencedQuerySource)
                //       ?? queryCompilationContext.Model.FindEntityType(entityParameter.Type);

                // if (trackingQuery)
                // {
                //     blockExpressions.Add(
                //         Expression.Call(
                //             Expression.Property(
                //                 EntityQueryModelVisitor.QueryContextParameter,
                //                 nameof(QueryContext.QueryBuffer)),
                //             _queryBufferStartTrackingMethodInfo,
                //             entityParameter,
                //             Expression.Constant(entityType)));
                // }

                var includedIndex = 0;

                blockExpressions.Add(GetLocalizationExpression(
                    queryCompilationContext,
                    propertyExpressions,
                    entityParameter,
                    trackingQuery,
                    asyncQuery,
                    ref includedIndex,
                    ref collectionIncludeId,
                    targetQuerySourceReferenceExpression));

                AwaitTaskExpressions(asyncQuery, blockExpressions);

                var includeExpression
                    = blockExpressions.Last().Type == typeof(Task)
                          ? new TaskBlockingExpressionVisitor()
                              .Visit(
                                  Expression.Call(
                                      _includeLocalizationAsyncMethodInfo.MakeGenericMethod(targetQuerySourceReferenceExpression.Type),
                                      EntityQueryModelVisitor.QueryContextParameter,
                                      targetQuerySourceReferenceExpression,
                                      Expression.NewArrayInit(typeof(object), propertyExpressions),
                                      Expression.Lambda(
                                          Expression.Block(blockExpressions),
                                          EntityQueryModelVisitor.QueryContextParameter,
                                          entityParameter,
                                          _includedParameter,
                                          CancellationTokenParameter),
                                      CancellationTokenParameter))
                          : Expression.Call(
                              _includeLocalizationMethodInfo.MakeGenericMethod(targetQuerySourceReferenceExpression.Type),
                              EntityQueryModelVisitor.QueryContextParameter,
                              targetQuerySourceReferenceExpression,
                              Expression.NewArrayInit(typeof(object), propertyExpressions),
                              Expression.Lambda(
                                  Expression.Block(typeof(void), blockExpressions),
                                  EntityQueryModelVisitor.QueryContextParameter,
                                  entityParameter,
                                  _includedParameter));

                var includeReplacingExpressionVisitor = new IncludeReplacingExpressionVisitor();

                queryModel.SelectClause.TransformExpressions(
                    e => includeReplacingExpressionVisitor.Replace(
                        targetQuerySourceReferenceExpression,
                        includeExpression,
                        e));
            }

            private Expression GetLocalizationExpression(
                QueryCompilationContext queryCompilationContext,
                ICollection<Expression> propertyExpressions,
                Expression targetEntityExpression,
                bool trackingQuery,
                bool asyncQuery,
                ref int includedIndex,
                ref int collectionIncludeId,
                Expression lastPropertyExpression)
            {
                var localizationPropertyInfo = lastPropertyExpression.Type.GetProperty("Localization");

                var entityType = queryCompilationContext.Model.FindEntityType(localizationPropertyInfo.PropertyType);
                var ignoredProperty = new IgnoredProperty("Localization", localizationPropertyInfo, entityType);
                // var relatedArrayAccessExpression = Expression.ArrayAccess(_includedParameter, Expression.Constant(includedIndex++));
                var cityLocalization = new CityLocalization();
                var nullExpression = Expression.Constant(cityLocalization);

                var blockExpressions = new List<Expression>();

                var stateManagerProperty
                    = Expression.Property(
                        EntityQueryModelVisitor.QueryContextParameter,
                        nameof(QueryContext.StateManager));

                blockExpressions.Add(
                    Expression.Call(
                        _setRelationshipSnapshotValueMethodInfo,
                        stateManagerProperty,
                        Expression.Constant(ignoredProperty),
                        Expression.New(entityType.ClrType.GetConstructors().First()),
                        nullExpression));

                var blockType = blockExpressions.Last().Type;

                return Expression.Block(blockType, blockExpressions);

                // propertyExpressions.Add(lastPropertyExpression = lastPropertyExpression.CreateEFPropertyExpression(localizationPropertyInfo));
                // 
                // 
                // 
                // var relatedEntityExpression = Expression.Convert(relatedArrayAccessExpression, localizationPropertyInfo.PropertyType);
                // 
                // 
                // 
                // var isNullBlockExpressions = new List<Expression>();
                // 
                // if (trackingQuery)
                // {
                //     blockExpressions.Add(
                //         Expression.Call(
                //             Expression.Property(
                //                 EntityQueryModelVisitor.QueryContextParameter,
                //                 nameof(QueryContext.QueryBuffer)),
                //             _queryBufferStartTrackingMethodInfo,
                //             relatedArrayAccessExpression,
                //             Expression.Constant(ignoredProperty.DeclaringEntityType)));
                // 
                //     blockExpressions.Add(
                //         Expression.Call(
                //             _setRelationshipSnapshotValueMethodInfo,
                //             stateManagerProperty,
                //             Expression.Constant(ignoredProperty),
                //             targetEntityExpression,
                //             relatedArrayAccessExpression));
                // 
                //     isNullBlockExpressions.Add(
                //         Expression.Call(
                //             _setRelationshipIsLoadedMethodInfo,
                //             stateManagerProperty,
                //             Expression.Constant(ignoredProperty),
                //             targetEntityExpression));
                // }
                // else
                // {
                //     blockExpressions.Add(
                //         targetEntityExpression
                //             .MakeMemberAccess(ignoredProperty.GetMemberInfo(false, true))
                //             .CreateAssignExpression(relatedEntityExpression));
                // }
                // 
                // AwaitTaskExpressions(asyncQuery, blockExpressions);
                // 
                // var blockType = blockExpressions.Last().Type;
                // 
                // isNullBlockExpressions.Add(
                //     blockType == typeof(Task)
                //         ? Expression.Constant(Task.CompletedTask)
                //         : (Expression)Expression.Default(blockType));
                // 
                // return
                //     Expression.Condition(
                //         Expression.Not(
                //             Expression.Call(
                //                 _referenceEqualsMethodInfo,
                //                 relatedArrayAccessExpression,
                //                 Expression.Constant(null, typeof(object)))),
                //         Expression.Block(
                //             blockType,
                //             blockExpressions),
                //         Expression.Block(
                //             blockType,
                //             isNullBlockExpressions),
                //         blockType);
            }

            protected static void ApplyIncludeExpressionsToQueryModel(
                QueryModel queryModel,
                QuerySourceReferenceExpression querySourceReferenceExpression,
                Expression expression)
            {
                var includeReplacingExpressionVisitor = new IncludeReplacingExpressionVisitor();

                foreach (var groupResultOperator
                    in queryModel.ResultOperators.OfType<GroupResultOperator>())
                {
                    var newElementSelector
                        = includeReplacingExpressionVisitor.Replace(
                            querySourceReferenceExpression,
                            expression,
                            groupResultOperator.ElementSelector);

                    if (!ReferenceEquals(newElementSelector, groupResultOperator.ElementSelector))
                    {
                        groupResultOperator.ElementSelector = newElementSelector;

                        return;
                    }
                }

                queryModel.SelectClause.TransformExpressions(
                    e => includeReplacingExpressionVisitor.Replace(
                        querySourceReferenceExpression,
                        expression,
                        e));
            }

            protected static void AwaitTaskExpressions(bool asyncQuery, List<Expression> blockExpressions)
            {
                if (asyncQuery)
                {
                    var taskExpressions = new List<Expression>();

                    foreach (var expression in blockExpressions.ToArray())
                    {
                        if (expression.Type == typeof(Task))
                        {
                            blockExpressions.Remove(expression);
                            taskExpressions.Add(expression);
                        }
                    }

                    if (taskExpressions.Count > 0)
                    {
                        blockExpressions.Add(
                            taskExpressions.Count == 1
                                ? taskExpressions[index: 0]
                                : Expression.Call(
                                    _awaitManyMethodInfo,
                                    Expression.NewArrayInit(
                                        typeof(Func<Task>),
                                        taskExpressions.Select(e => Expression.Lambda(e)))));
                    }
                }
            }

            private static readonly MethodInfo _awaitManyMethodInfo
                = typeof(IncludeLocalizationLoadTreeNodeBase).GetTypeInfo()
                    .GetDeclaredMethod(nameof(_AwaitMany));

            // ReSharper disable once InconsistentNaming
            private static async Task _AwaitMany(IReadOnlyList<Func<Task>> taskFactories)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < taskFactories.Count; i++)
                {
                    await taskFactories[i]();
                }
            }

            private class IncludeReplacingExpressionVisitor : RelinqExpressionVisitor
            {
                private QuerySourceReferenceExpression _querySourceReferenceExpression;
                private Expression _includeExpression;

                public Expression Replace(
                    QuerySourceReferenceExpression querySourceReferenceExpression,
                    Expression includeExpression,
                    Expression searchedExpression)
                {
                    _querySourceReferenceExpression = querySourceReferenceExpression;
                    _includeExpression = includeExpression;

                    return Visit(searchedExpression);
                }

                protected override Expression VisitQuerySourceReference(
                    QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    if (ReferenceEquals(querySourceReferenceExpression, _querySourceReferenceExpression))
                    {
                        _querySourceReferenceExpression = null;

                        return _includeExpression;
                    }

                    return querySourceReferenceExpression;
                }
            }
        }

        private sealed class IncludeLocalizationLoadTreeNode : IncludeLocalizationLoadTreeNodeBase
        {
            private static readonly MethodInfo _referenceEqualsMethodInfo
                = typeof(object).GetTypeInfo()
                    .GetDeclaredMethod(nameof(ReferenceEquals));

            private static readonly MethodInfo _collectionAccessorAddMethodInfo
                = typeof(IClrCollectionAccessor).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

            private static readonly MethodInfo _queryBufferIncludeCollectionMethodInfo
                = typeof(IQueryBuffer).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollection));

            private static readonly MethodInfo _queryBufferIncludeCollectionAsyncMethodInfo
                = typeof(IQueryBuffer).GetTypeInfo()
                    .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollectionAsync));

            public IncludeLocalizationLoadTreeNode(INavigation navigation) => Navigation = navigation;

            public INavigation Navigation { get; }

            public Expression Compile(
                QueryCompilationContext queryCompilationContext,
                Expression targetQuerySourceReferenceExpression,
                Expression entityParameter,
                ICollection<Expression> propertyExpressions,
                bool trackingQuery,
                bool asyncQuery,
                ref int includedIndex,
                ref int collectionIncludeId)
                => Navigation.IsCollection()
                       ? CompileCollectionInclude(
                           queryCompilationContext,
                           targetQuerySourceReferenceExpression,
                           entityParameter,
                           trackingQuery,
                           asyncQuery,
                           ref collectionIncludeId)
                       : CompileReferenceInclude(
                           queryCompilationContext,
                           propertyExpressions,
                           entityParameter,
                           trackingQuery,
                           asyncQuery,
                           ref includedIndex,
                           ref collectionIncludeId,
                           targetQuerySourceReferenceExpression);

            private Expression CompileCollectionInclude(
                QueryCompilationContext queryCompilationContext,
                Expression targetExpression,
                Expression entityParameter,
                bool trackingQuery,
                bool asyncQuery,
                ref int collectionIncludeId)
            {
                int collectionId;

                if (targetExpression is QuerySourceReferenceExpression targetQuerySourceReferenceExpression
                    && targetQuerySourceReferenceExpression.ReferencedQuerySource is IFromClause fromClause
                    && fromClause.FromExpression is QuerySourceReferenceExpression fromClauseQuerySourceReferenceExpression
                    && fromClauseQuerySourceReferenceExpression.ReferencedQuerySource is GroupJoinClause)
                {
                    // -1 == unable to optimize (GJ)

                    collectionId = -1;
                }
                else
                {
                    collectionId = collectionIncludeId++;
                }

                var targetEntityType = Navigation.GetTargetType();
                var targetType = targetEntityType.ClrType;

                var mainFromClause
                    = new MainFromClause(
                        targetType.Name.Substring(0, 1).ToLowerInvariant(),
                        targetType,
                        targetExpression.CreateEFPropertyExpression(Navigation));

                queryCompilationContext.AddQuerySourceRequiringMaterialization(mainFromClause);

                var querySourceReferenceExpression
                    = new QuerySourceReferenceExpression(mainFromClause);

                queryCompilationContext.AddOrUpdateMapping(mainFromClause, targetEntityType);

                var collectionQueryModel
                    = new QueryModel(
                        mainFromClause,
                        new SelectClause(querySourceReferenceExpression));

                Compile(
                    queryCompilationContext,
                    collectionQueryModel,
                    trackingQuery,
                    asyncQuery,
                    ref collectionIncludeId,
                    querySourceReferenceExpression);

                Expression collectionLambdaExpression
                    = Expression.Lambda(new SubQueryExpression(collectionQueryModel));

                var includeCollectionMethodInfo = _queryBufferIncludeCollectionMethodInfo;

                Expression cancellationTokenExpression = null;

                if (asyncQuery)
                {
                    var asyncEnumerableType
                        = typeof(IAsyncEnumerable<>).MakeGenericType(targetType);

                    collectionLambdaExpression
                        = Expression.Convert(
                            collectionLambdaExpression,
                            typeof(Func<>).MakeGenericType(asyncEnumerableType));

                    includeCollectionMethodInfo = _queryBufferIncludeCollectionAsyncMethodInfo;
                    cancellationTokenExpression = CancellationTokenParameter;
                }

                return
                    BuildCollectionIncludeExpressions(
                        Navigation,
                        entityParameter,
                        trackingQuery,
                        collectionLambdaExpression,
                        includeCollectionMethodInfo,
                        cancellationTokenExpression,
                        collectionId);
            }

            private static Expression BuildCollectionIncludeExpressions(
                INavigation navigation,
                Expression targetEntityExpression,
                bool trackingQuery,
                Expression relatedCollectionFuncExpression,
                MethodInfo includeCollectionMethodInfo,
                Expression cancellationTokenExpression,
                int collectionIncludeId)
            {
                var inverseNavigation = navigation.FindInverse();
                var clrCollectionAccessor = navigation.GetCollectionAccessor();

                var arguments = new List<Expression>
                {
                    Expression.Constant(collectionIncludeId),
                    Expression.Constant(navigation),
                    Expression.Constant(inverseNavigation, typeof(INavigation)),
                    Expression.Constant(navigation.GetTargetType()),
                    Expression.Constant(clrCollectionAccessor),
                    Expression.Constant(inverseNavigation?.GetSetter(), typeof(IClrPropertySetter)),
                    Expression.Constant(trackingQuery),
                    targetEntityExpression,
                    relatedCollectionFuncExpression,
                    TryCreateJoinPredicate(targetEntityExpression.Type, navigation)
                };

                if (cancellationTokenExpression != null)
                {
                    arguments.Add(cancellationTokenExpression);
                }

                var targetClrType = navigation.GetTargetType().ClrType;

                var includeCollectionMethodCall
                    = Expression.Call(
                        Expression.Property(
                            EntityQueryModelVisitor.QueryContextParameter,
                            nameof(QueryContext.QueryBuffer)),
                        includeCollectionMethodInfo
                            .MakeGenericMethod(
                                targetEntityExpression.Type,
                                targetClrType,
                                clrCollectionAccessor.CollectionType.TryGetSequenceType()
                                ?? targetClrType),
                        arguments);

                return
                    navigation.DeclaringEntityType.BaseType != null
                        ? Expression.Condition(
                            Expression.TypeIs(
                                targetEntityExpression,
                                navigation.DeclaringType.ClrType),
                            includeCollectionMethodCall,
                            includeCollectionMethodInfo.ReturnType == typeof(Task)
                                ? (Expression)Expression.Constant(Task.CompletedTask)
                                : Expression.Default(includeCollectionMethodInfo.ReturnType))
                        : (Expression)includeCollectionMethodCall;
            }

            private static Expression TryCreateJoinPredicate(Type targetType, INavigation navigation)
            {
                var foreignKey = navigation.ForeignKey;
                var primaryKeyProperties = foreignKey.PrincipalKey.Properties;
                var foreignKeyProperties = foreignKey.Properties;
                var relatedType = navigation.GetTargetType().ClrType;

                if (primaryKeyProperties.Any(p => p.IsShadowProperty)
                    || foreignKeyProperties.Any(p => p.IsShadowProperty))
                {
                    return
                        Expression.Default(
                            typeof(Func<,,>)
                                .MakeGenericType(targetType, relatedType, typeof(bool)));
                }

                var targetEntityParameter = Expression.Parameter(targetType, "p");
                var relatedEntityParameter = Expression.Parameter(relatedType, "d");

                return Expression.Lambda(
                    primaryKeyProperties.Zip(
                            foreignKeyProperties,
                            (pk, fk) =>
                            {
                                Expression pkMemberAccess
                                    = Expression.MakeMemberAccess(
                                        targetEntityParameter,
                                        pk.GetMemberInfo(forConstruction: false, forSet: false));

                                if (pkMemberAccess.Type != pk.ClrType)
                                {
                                    pkMemberAccess = Expression.Convert(pkMemberAccess, pk.ClrType);
                                }

                                Expression fkMemberAccess
                                    = Expression.MakeMemberAccess(
                                        relatedEntityParameter,
                                        fk.GetMemberInfo(forConstruction: false, forSet: false));

                                if (fkMemberAccess.Type != fk.ClrType)
                                {
                                    fkMemberAccess = Expression.Convert(fkMemberAccess, fk.ClrType);
                                }

                                if (pkMemberAccess.Type != fkMemberAccess.Type)
                                {
                                    if (pkMemberAccess.Type.IsNullableType())
                                    {
                                        fkMemberAccess = Expression.Convert(fkMemberAccess, pkMemberAccess.Type);
                                    }
                                    else
                                    {
                                        pkMemberAccess = Expression.Convert(pkMemberAccess, fkMemberAccess.Type);
                                    }
                                }

                                Expression equalityExpression;

                                var comparer
                                    = pk.GetKeyValueComparer()
                                      ?? pk.FindMapping()?.KeyComparer;

                                if (comparer != null)
                                {
                                    if (comparer.Type != pkMemberAccess.Type
                                        && comparer.Type == pkMemberAccess.Type.UnwrapNullableType())
                                    {
                                        comparer = comparer.ToNonNullNullableComparer();
                                    }

                                    equalityExpression
                                        = comparer.ExtractEqualsBody(
                                            pkMemberAccess,
                                            fkMemberAccess);
                                }
                                else if (typeof(IStructuralEquatable).GetTypeInfo()
                                    .IsAssignableFrom(pkMemberAccess.Type.GetTypeInfo()))
                                {
                                    equalityExpression
                                        = Expression.Call(_structuralEqualsMethod, pkMemberAccess, fkMemberAccess);
                                }
                                else
                                {
                                    equalityExpression = Expression.Equal(pkMemberAccess, fkMemberAccess);
                                }

                                return fk.ClrType.IsNullableType()
                                           ? Expression.Condition(
                                               Expression.Equal(fkMemberAccess, Expression.Default(fk.ClrType)),
                                               Expression.Constant(false),
                                               equalityExpression)
                                           : equalityExpression;
                            })
                        .Aggregate(Expression.AndAlso),
                    targetEntityParameter,
                    relatedEntityParameter);
            }

            private static readonly MethodInfo _structuralEqualsMethod
                = typeof(IncludeLocalizationLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(StructuralEquals));

            private static bool StructuralEquals(object x, object y)
            {
                return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
            }

            private Expression CompileReferenceInclude(
                QueryCompilationContext queryCompilationContext,
                ICollection<Expression> propertyExpressions,
                Expression targetEntityExpression,
                bool trackingQuery,
                bool asyncQuery,
                ref int includedIndex,
                ref int collectionIncludeId,
                Expression lastPropertyExpression)
            {
                propertyExpressions.Add(
                    lastPropertyExpression
                        = lastPropertyExpression.CreateEFPropertyExpression(Navigation));

                var relatedArrayAccessExpression
                    = Expression.ArrayAccess(_includedParameter, Expression.Constant(includedIndex++));

                var relatedEntityExpression
                    = Expression.Convert(relatedArrayAccessExpression, Navigation.ClrType);

                var stateManagerProperty
                    = Expression.Property(
                        EntityQueryModelVisitor.QueryContextParameter,
                        nameof(QueryContext.StateManager));

                var blockExpressions = new List<Expression>();
                var isNullBlockExpressions = new List<Expression>();

                if (trackingQuery)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            Expression.Property(
                                EntityQueryModelVisitor.QueryContextParameter,
                                nameof(QueryContext.QueryBuffer)),
                            _queryBufferStartTrackingMethodInfo,
                            relatedArrayAccessExpression,
                            Expression.Constant(Navigation.GetTargetType())));

                    blockExpressions.Add(
                        Expression.Call(
                            _setRelationshipSnapshotValueMethodInfo,
                            stateManagerProperty,
                            Expression.Constant(Navigation),
                            targetEntityExpression,
                            relatedArrayAccessExpression));

                    isNullBlockExpressions.Add(
                        Expression.Call(
                            _setRelationshipIsLoadedMethodInfo,
                            stateManagerProperty,
                            Expression.Constant(Navigation),
                            targetEntityExpression));
                }
                else
                {
                    blockExpressions.Add(
                        targetEntityExpression
                            .MakeMemberAccess(Navigation.GetMemberInfo(false, true))
                            .CreateAssignExpression(relatedEntityExpression));
                }

                var inverseNavigation = Navigation.FindInverse();

                if (inverseNavigation != null)
                {
                    var collection = inverseNavigation.IsCollection();

                    if (trackingQuery)
                    {
                        blockExpressions.Add(
                            Expression.Call(
                                collection
                                    ? _addToCollectionSnapshotMethodInfo
                                    : _setRelationshipSnapshotValueMethodInfo,
                                stateManagerProperty,
                                Expression.Constant(inverseNavigation),
                                relatedArrayAccessExpression,
                                targetEntityExpression));
                    }
                    else
                    {
                        blockExpressions.Add(
                            collection
                                ? (Expression)Expression.Call(
                                    Expression.Constant(inverseNavigation.GetCollectionAccessor()),
                                    _collectionAccessorAddMethodInfo,
                                    relatedArrayAccessExpression,
                                    targetEntityExpression)
                                : relatedEntityExpression.MakeMemberAccess(
                                        inverseNavigation.GetMemberInfo(forConstruction: false, forSet: true))
                                    .CreateAssignExpression(targetEntityExpression));
                    }
                }

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var includeLoadTreeNode in Children)
                {
                    blockExpressions.Add(
                        includeLoadTreeNode.Compile(
                            queryCompilationContext,
                            lastPropertyExpression,
                            relatedEntityExpression,
                            propertyExpressions,
                            trackingQuery,
                            asyncQuery,
                            ref includedIndex,
                            ref collectionIncludeId));
                }

                AwaitTaskExpressions(asyncQuery, blockExpressions);

                var blockType = blockExpressions.Last().Type;

                isNullBlockExpressions.Add(
                    blockType == typeof(Task)
                        ? Expression.Constant(Task.CompletedTask)
                        : (Expression)Expression.Default(blockType));

                return
                    Expression.Condition(
                        Expression.Not(
                            Expression.Call(
                                _referenceEqualsMethodInfo,
                                relatedArrayAccessExpression,
                                Expression.Constant(null, typeof(object)))),
                        Expression.Block(
                            blockType,
                            blockExpressions),
                        Expression.Block(
                            blockType,
                            isNullBlockExpressions),
                        blockType);
            }

            private static readonly MethodInfo _setRelationshipSnapshotValueMethodInfo
                = typeof(IncludeLocalizationLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(SetRelationshipSnapshotValue));

            private static void SetRelationshipSnapshotValue(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity,
                object value)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.SetRelationshipSnapshotValue(navigation, value);
            }

            private static readonly MethodInfo _setRelationshipIsLoadedMethodInfo
                = typeof(IncludeLocalizationLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(SetRelationshipIsLoaded));

            private static void SetRelationshipIsLoaded(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.SetIsLoaded((INavigation)navigation);
            }

            private static readonly MethodInfo _addToCollectionSnapshotMethodInfo
                = typeof(IncludeLocalizationLoadTreeNode).GetTypeInfo()
                    .GetDeclaredMethod(nameof(AddToCollectionSnapshot));

            private static void AddToCollectionSnapshot(
                IStateManager stateManager,
                IPropertyBase navigation,
                object entity,
                object value)
            {
                var internalEntityEntry = stateManager.TryGetEntry(entity);

                Debug.Assert(internalEntityEntry != null);

                internalEntityEntry.AddToCollectionSnapshot(navigation, value);
            }
        }

        private sealed class IncludeLocalizationLoadTree : IncludeLocalizationLoadTreeNodeBase
        {
            public IncludeLocalizationLoadTree(QuerySourceReferenceExpression querySourceReferenceExpression)
                => QuerySourceReferenceExpression = querySourceReferenceExpression;

            public QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }

            public void AddLoadPath(IReadOnlyList<INavigation> navigationPath)
            {
                AddLoadPath(this, navigationPath, index: 0);
            }

            public void Compile(
                QueryCompilationContext queryCompilationContext,
                QueryModel queryModel,
                bool trackingQuery,
                bool asyncQuery,
                ref int collectionIncludeId)
            {
                var querySourceReferenceExpression = QuerySourceReferenceExpression;

                // if (querySourceReferenceExpression.ReferencedQuerySource is GroupJoinClause groupJoinClause)
                // {
                //     if (queryModel.GetOutputExpression() is SubQueryExpression subQueryExpression
                //         && subQueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression qsre
                //         && (qsre.ReferencedQuerySource as MainFromClause)?.FromExpression == QuerySourceReferenceExpression)
                //     {
                //         querySourceReferenceExpression = qsre;
                //         queryModel = subQueryExpression.QueryModel;
                //     }
                //     else
                //     {
                //         // We expand GJs to 'from e in [g] select e' so we can rewrite the projector
                // 
                //         var joinClause = groupJoinClause.JoinClause;
                // 
                //         var mainFromClause
                //             = new MainFromClause(joinClause.ItemName, joinClause.ItemType, QuerySourceReferenceExpression);
                // 
                //         querySourceReferenceExpression = new QuerySourceReferenceExpression(mainFromClause);
                // 
                //         var subQueryModel
                //             = new QueryModel(
                //                 mainFromClause,
                //                 new SelectClause(querySourceReferenceExpression));
                // 
                //         ApplyIncludeExpressionsToQueryModel(
                //             queryModel, QuerySourceReferenceExpression, new SubQueryExpression(subQueryModel));
                // 
                //         queryModel = subQueryModel;
                //     }
                // }
                Compile(
                    queryCompilationContext,
                    queryModel,
                    trackingQuery,
                    asyncQuery,
                    ref collectionIncludeId,
                    querySourceReferenceExpression);
            }
        }
    }
}
