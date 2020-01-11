// ReSharper disable All

namespace LocalizationTest.Data.EntityFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LocalizationTest.Data.Core.Abstraction;
    using LocalizationTest.Data.EntityFramework.Utilities.ExpressionVisitors;

    using Microsoft.EntityFrameworkCore.Query.Internal;

    public static class LocalizableExtensions
    {
        public static ILocalizableQueryable<TEntity, TProperty> IncludeLocalization<TEntity, TProperty>(this IQueryable<TEntity> source, Expression<Func<TEntity, TProperty>> navigationPropertyPath)
        {
            return new LocalizableQuery<TEntity, TProperty>(
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: IncludeLocalizationMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
                            arguments: new[]
                            {
                                source.Expression,
                                Expression.Quote(navigationPropertyPath)
                            }))
                    : source);
        }

        public static IEnumerable<TEntity> TransformLocalizationResult<TLocalizationResult, TLocalization, TEntity>(
            this IEnumerable<TLocalizationResult> source,
            Expression<Func<TEntity, TLocalization>> localizationSelector)
            where TEntity : class, IBaseEntity<Guid>
            where TLocalization : ILocalization<ILocalizable<Guid>>
            where TLocalizationResult : ILocalizationResult<Guid, TEntity, TLocalization>
        {
            var resultParameter = Expression.Parameter(typeof(TLocalizationResult), "result");

            var entitySelector = Expression.Property(resultParameter, nameof(ILocalizationResult<Guid, TEntity, TLocalization>.Entity));
            var entitylocalizationSelector = new ReplaceVisitor(localizationSelector.Parameters.First(), entitySelector).Visit(localizationSelector.Body);
            var resultBusinessLocalizationSelector = Expression.Property(resultParameter, nameof(ILocalizationResult<Guid, TEntity, TLocalization>.BusinessLocalization));
            var resultUserLocalizationSelector = Expression.Property(resultParameter, nameof(ILocalizationResult<Guid, TEntity, TLocalization>.UserLocalization));
            var coalesce = Expression.Coalesce(resultUserLocalizationSelector, resultBusinessLocalizationSelector);
            var assignment = Expression.Assign(entitylocalizationSelector, coalesce);
            var transformLambda = Expression.Lambda<Action<TLocalizationResult>>(assignment, resultParameter);
            var transformHandler = transformLambda.Compile();


            foreach (var item in source)
            {
                transformHandler(item);
                yield return item.Entity;
            }
        }

        public static readonly MethodInfo IncludeLocalizationMethodInfo = typeof(LocalizableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(IncludeLocalization))
            .Single(
                mi => mi.GetGenericArguments()
                          .Count()
                      == 2
                      && mi.GetParameters()
                          .Any(pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));
    }

    public interface ILocalizableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
    {
    }

    public class LocalizableQuery<TEntity, TProperty> : ILocalizableQueryable<TEntity, TProperty>, IAsyncEnumerable<TEntity>
    {
        private readonly IQueryable<TEntity> _queryable;

        public LocalizableQuery(IQueryable<TEntity> queryable)
        {
            _queryable = queryable;
        }

        public Expression Expression => _queryable.Expression;

        public Type ElementType => _queryable.ElementType;

        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<TEntity> GetEnumerator() => _queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator() => ((IAsyncEnumerable<TEntity>)_queryable).GetEnumerator();
    }
}
