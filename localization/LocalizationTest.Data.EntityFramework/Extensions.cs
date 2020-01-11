// ReSharper disable UnusedMember.Global
// ReSharper disable StyleCop.SA1402
// ReSharper disable StyleCop.SA1204

// ReSharper disable All
namespace LocalizationTest.Data.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LocalizationTest.Data.Core.Abstraction;
    using LocalizationTest.Data.Core.Entities;
    using LocalizationTest.Data.EntityFramework.Base.Entities;
    using LocalizationTest.Data.EntityFramework.Models;
    using LocalizationTest.Data.EntityFramework.Utilities.ExpressionVisitors;

    using Microsoft.EntityFrameworkCore.Query.Internal;

    public static class Extensions
    {
        public static IQueryable<TEntity> IncludeLocalization<TEntity, TLocalizable>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, TLocalizable>> localizableSelector,
            Guid businessCultureId,
            Guid userCultureId)
        where TLocalizable : ILocalizable<Guid>
        {
            Type localizationType = typeof(TLocalizable).BaseType?.GetGenericArguments().First();
            MethodInfo method = typeof(Extensions).GetMethod(nameof(IncludeLocalization), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo genericMethod = method?.MakeGenericMethod(
                typeof(TEntity),
                typeof(TLocalizable),
                localizationType);

            object[] parameters =
                {
                    source,
                    localizableSelector,
                    businessCultureId,
                    userCultureId
                };

            return (IQueryable<TEntity>)genericMethod?.Invoke(null, parameters);
        }

        private static IQueryable<TEntity> IncludeLocalization<TEntity, TLocalizable, TLocalization>(
             this IQueryable<TEntity> source,
             Expression<Func<TEntity, TLocalizable>> localizableSelector,
             Guid businessCultureId,
             Guid userCultureId)
            where TEntity : BaseEntity<Guid>, new()
            where TLocalizable : class, ILocalizable<Guid>, new()
        {

            Type joinKeyType = new { LocalizableId = Guid.Empty, CultureId = Guid.Empty }.GetType();
            Type businessGroupResultType = new { Entity = default(TEntity), BusinessLocalizations = default(IEnumerable<TLocalization>) }.GetType();
            Type businessSelectResultType = new { Entity = default(TEntity), BusinessLocalization = default(TLocalization) }.GetType();
            Type userGroupResultType = new { Entity = default(TEntity), BusinessLocalization = default(TLocalization), UserLocalizations = default(IEnumerable<TLocalization>) }.GetType();
            Type userSelectResultType = new { Entity = default(TEntity), BusinessLocalization = default(TLocalization), UserLocalization = default(TLocalization) }.GetType();

            MethodInfo method = typeof(Extensions).GetMethod(nameof(IncludeLocalizationPath), BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo genericMethod = method?.MakeGenericMethod(
                typeof(TEntity),
                typeof(TLocalizable),
                typeof(TLocalization),
                joinKeyType,
                businessGroupResultType,
                businessSelectResultType,
                userGroupResultType,
                userSelectResultType);

            object[] parameters =
                {
                     source,
                     localizableSelector,
                     businessCultureId,
                     userCultureId
                 };

            return (IQueryable<TEntity>)genericMethod?.Invoke(null, parameters);
        }

        private static IQueryable<TEntity> IncludeLocalizationPath<TEntity, 
                                                                   TLocalizable, 
                                                                   TLocalization, 
                                                                   TJoinKey, 
                                                                   TBusinessGroupResult, 
                                                                   TBusinessSelectResult,
                                                                   TUserGroupResult,
                                                                   TUserSelectResult>(
            this IQueryable<TEntity> source,
            Expression<Func<TEntity, TLocalizable>> localizableSelectorLambda,
            Guid businessCultureId,
            Guid userCultureId)
            where TEntity : BaseEntity<Guid>, new()
            where TLocalizable : Localizable<TLocalization>, new() 
            where TLocalization : class, ILocalization<Guid>, new()
        {
            EntityQueryable<TLocalization> localizationEntity = new EntityQueryable<TLocalization>((IAsyncQueryProvider)source.Provider);
            var localizationQuery = source.Provider.CreateQuery<TLocalization>(Expression.Constant(localizationEntity));

            // var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(source.Provider);
            // var database = DataBaseField.GetValue(queryCompiler);
            // var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
            // var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);


            // var parameter = Expression.Parameter(typeof(LocalizationResult<TEntity, TLocalization>), "result");
            // var entitySelector = Expression.Property(parameter, "Entity");
            // var fullLocalizableSelector = new ReplaceVisitor(localizableSelectorLambda.Parameters.First(), entitySelector)
            //     .Visit(localizableSelectorLambda.Body);
            // var businessLocalizationProperty = Expression.Property(parameter, "BusinessLocalization");
            // var userLocalizationProperty = Expression.Property(parameter, "UserLocalization");
            // 
            // var bindings = typeof(TLocalizable).GetProperties()
            //     .Where(propertyInfo => propertyInfo.Name != "Localization" && propertyInfo.Name != "Localizations")
            //     .Select(
            //         propertyInfo => Expression.Bind(
            //             propertyInfo,
            //             Expression.Property(fullLocalizableSelector, propertyInfo)))
            //     .ToList();
            // 
            // var localizationBinding = Expression.Bind(
            //     typeof(TLocalizable).GetProperty("Localization") ?? throw new InvalidOperationException(),
            //     Expression.Coalesce(userLocalizationProperty, businessLocalizationProperty));
            // 
            // bindings.Add(localizationBinding);
            // 
            // var memberInitExpression = Expression.MemberInit(Expression.New(typeof(TLocalizable)), bindings);
            // var selectLambda =
            //     Expression.Lambda<Func<LocalizationResult<TLocalizable, TLocalization>, TLocalizable>>(
            //         memberInitExpression,
            //         parameter);
            //Type joinType = new { LocalizableId = Guid.Empty, CultureId = Guid.Empty }.GetType();
            //Type resultType = new { Entity = new TEntity(), BusinessLocalization = new TLocalization() }.GetType();

            // Defineing all parameters
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var localizationParameter = Expression.Parameter(typeof(TLocalization), "localization");
            var localizationsParameter = Expression.Parameter(typeof(IEnumerable<TLocalization>), "localizations");
            var businessGroupResultParameter = Expression.Parameter(typeof(TBusinessGroupResult), "result");
            //userGroupResultParameter
            var businessSelectResultParameter = Expression.Parameter(typeof(TBusinessSelectResult), "result");
            //var localizationObjectParameter = Expression.Parameter(typeof(object), "localization");
            var userGroupResultParameter = Expression.Parameter(typeof(TUserGroupResult), "result");
            var userSelectResultParameter = Expression.Parameter(typeof(TUserSelectResult), "result");

            // Defining all parameter property accessors
            var entityParameterLocalizableSelector = new ReplaceVisitor(localizableSelectorLambda.Parameters.First(), entityParameter).Visit(localizableSelectorLambda.Body);
            var entityParameterLocalizableIdSelector = Expression.Property(entityParameterLocalizableSelector ?? throw new InvalidOperationException(), nameof(ILocalizable<Guid>.Id));

            var businessSelectResultParameterEntitySelector = Expression.Property(businessSelectResultParameter, "Entity");
            var businessSelectResultParameterBusinessLocalizationSelector = Expression.Property(businessSelectResultParameter, "BusinessLocalization");
            var businessSelectResultParameterLocalizableSelector = new ReplaceVisitor(localizableSelectorLambda.Parameters.First(), businessSelectResultParameterEntitySelector).Visit(localizableSelectorLambda.Body);
            var businessSelectResultParameterLocalizableIdSelector = Expression.Property(businessSelectResultParameterLocalizableSelector ?? throw new InvalidOperationException(), nameof(ILocalizable<Guid>.Id));


            // Defining all dynamic types constructor infos
            var joinKeyConstructorInfo = typeof(TJoinKey).GetConstructors().First();
            var businessSelectResultConstructorInfo = typeof(TBusinessSelectResult).GetConstructors().First();
            var userSelectResultConstructorInfo = typeof(TUserSelectResult).GetConstructors().First();
            var userGroupResultConstructorInfo = typeof(TUserGroupResult).GetConstructors().First();



            var memberInfos = typeof(TJoinKey).GetProperties();

            // Business localizable key
            var businessLocalizableKeyArguemnts = new Expression[] { entityParameterLocalizableIdSelector, Expression.Constant(businessCultureId) };
            var businessLocalizableKeySelector = Expression.New(joinKeyConstructorInfo, businessLocalizableKeyArguemnts, memberInfos);
            var businessLocalizableKeyLambda = Expression.Lambda<Func<TEntity, object>>(businessLocalizableKeySelector, entityParameter);

            // Business localization key
            var businessLocalizationKeyArguemnts = new Expression[]
                                               {
                                                   Expression.Property(localizationParameter, nameof(ILocalization<Guid>.LocalizableId)),
                                                   Expression.Property(localizationParameter, nameof(ILocalization<Guid>.CultureId))
                                               };
            var businessLocalizationKeySelector = Expression.New(joinKeyConstructorInfo, businessLocalizationKeyArguemnts, memberInfos);
            var businessLocalizationKeyLambda = Expression.Lambda<Func<TLocalization, object>>(businessLocalizationKeySelector, localizationParameter);

            // User localizable key
            var userLocalizableKeyArguemnts = new Expression[] { businessSelectResultParameterLocalizableIdSelector, Expression.Constant(userCultureId) };
            var userLocalizableKeySelector = Expression.New(joinKeyConstructorInfo, userLocalizableKeyArguemnts, memberInfos);
            var userlocalizableKeyLambda = Expression.Lambda<Func<TBusinessSelectResult, object>>(userLocalizableKeySelector, businessSelectResultParameter);

            // User localization key
            var userLocalizationKeyArguemnts = new Expression[]
                                                       {
                                                           Expression.Property(localizationParameter, nameof(ILocalization<Guid>.LocalizableId)),
                                                           Expression.Property(localizationParameter, nameof(ILocalization<Guid>.CultureId))
                                                       };
            var userLocalizationKeySelector = Expression.New(joinKeyConstructorInfo, userLocalizationKeyArguemnts, memberInfos);
            var userLocalizationKeyLambda = Expression.Lambda<Func<TLocalization, object>>(userLocalizationKeySelector, localizationParameter);


            // business group result selector
            // (entity, localizations) => new { Entity = entity, BusinessLocalizations = localizations} )
            var businessGroupResultArguemnts = new Expression[] { entityParameter, localizationsParameter};
            var businessGroupResultMemberInfos = typeof(TBusinessGroupResult).GetProperties();
            var businessGroupResultSelector = Expression.Lambda<Func<TEntity, IEnumerable<TLocalization>, TBusinessGroupResult>>(
                Expression.New(
                    typeof(TBusinessGroupResult).GetConstructors().First(),
                    businessGroupResultArguemnts,
                    businessGroupResultMemberInfos),
                entityParameter,
                localizationsParameter);

            // business select default if empty
            // result => result.Localizations.DefaultIfEmpty()
            var businessSelectDefaultIfEmpty = Expression.Lambda<Func<TBusinessGroupResult, IEnumerable<TLocalization>>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.DefaultIfEmpty),
                    new Type[] { typeof(TLocalization) },
                    Expression.Property(businessGroupResultParameter, "BusinessLocalizations")),
                businessGroupResultParameter);

            // business select result selector - its broken, i should fix it (Expression body is not correct)
            // (result, localization) => new { result.Entity, BusinessLocalization = localization })
            var businessGroupResultEntitySelector = Expression.Property(businessGroupResultParameter, "Entity");
            var businessSelectResultArguemnts = new Expression[] { businessGroupResultEntitySelector, localizationParameter };
            var businessSelectResultMemberInfos = typeof(TBusinessSelectResult).GetProperties();
            var businessSelectResultSelector = Expression.Lambda<Func<TBusinessGroupResult, TLocalization, TBusinessSelectResult>>(
                Expression.New(businessSelectResultConstructorInfo,
                    businessSelectResultArguemnts,
                    businessSelectResultMemberInfos),
                businessGroupResultParameter,
                localizationParameter);

            // user group result selector
            // (result, localizations) => new { Entity = result.Entity, BusinessLocalization = result.BusinessLocalization, UserLocalizations = localizations} )
            var userGroupResultArguemnts = new Expression[]
                                               {
                                                   businessSelectResultParameterEntitySelector, 
                                                   businessSelectResultParameterBusinessLocalizationSelector,
                                                   localizationsParameter
                                               };
            var userGroupResultMemberInfos = typeof(TUserGroupResult).GetProperties();
            var userGroupResultSelector = Expression.Lambda<Func<TBusinessSelectResult, IEnumerable<TLocalization>, TUserGroupResult>>(
                Expression.New(
                    userGroupResultConstructorInfo,
                    userGroupResultArguemnts,
                    userGroupResultMemberInfos),
                businessSelectResultParameter,
                localizationsParameter);

            // user select default if empty
            // result => result.UserLocalizationList.DefaultIfEmpty(),
            var userSelectDefaultIfEmpty = Expression.Lambda<Func<TUserGroupResult, IEnumerable<TLocalization>>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.DefaultIfEmpty),
                    new Type[] { typeof(TLocalization) },
                    Expression.Property(userGroupResultParameter, "UserLocalizations")),
                userGroupResultParameter);

            // business select result selector - its broken, i should fix it (Expression body is not correct)
            // (result, localization) => new { result.Entity, BusinessLocalization = result.BusinessLocalization, UserLocalization = localization })
            var userGroupResultEntitySelector = Expression.Property(userGroupResultParameter, "Entity");
            var userGroupResultBusinessLocalizationSelector = Expression.Property(userGroupResultParameter, "BusinessLocalization");
            var userSelectResultArguemnts = new Expression[] { userGroupResultEntitySelector, userGroupResultBusinessLocalizationSelector, localizationParameter };
            var userSelectResultMemberInfos = typeof(TUserSelectResult).GetProperties();
            var userSelectResultSelector = Expression.Lambda<Func<TUserGroupResult, TLocalization, TUserSelectResult>>(
                Expression.New(userSelectResultConstructorInfo,
                    userSelectResultArguemnts,
                    userSelectResultMemberInfos),
                userGroupResultParameter,
                localizationParameter);

            // Doesnt work
            var resultEntity = Expression.Property(userSelectResultParameter, "Entity");
            var resultLocalizable = new ReplaceVisitor(localizableSelectorLambda.Parameters.First(), resultEntity).Visit(localizableSelectorLambda.Body);
            var resultLocalization = Expression.Property(resultLocalizable, "Localization");
            var resultBusinessLocalization = Expression.Property(userSelectResultParameter, "BusinessLocalization");
            var resultUserLocalization = Expression.Property(userSelectResultParameter, "UserLocalization");
            var resultLocalizationCoalesce = Expression.Coalesce(resultBusinessLocalization, resultUserLocalization);
            var resultLocalizationAssignment = Expression.Assign(resultLocalization, resultLocalizationCoalesce);
            var actionBlock = Expression.Block(typeof(TEntity), resultLocalizationAssignment, resultEntity);
            var actionBlockLambda = Expression.Lambda<Func<TUserSelectResult, TEntity>>(actionBlock, userSelectResultParameter);

            // Simple Selector
            var entitySelectLambda = Expression.Lambda<Func<TUserSelectResult, TEntity>>(resultEntity, userSelectResultParameter);

            // TestArea
            var finalResult = source
                .GroupJoin(
                    localizationQuery,
                    businessLocalizableKeyLambda,
                    businessLocalizationKeyLambda,
                    businessGroupResultSelector)
                .SelectMany(
                    businessSelectDefaultIfEmpty,
                    businessSelectResultSelector)
                .GroupJoin(
                     localizationQuery, 
                     userlocalizableKeyLambda, //result => new { LocalizableId = result.Entity.Id, CultureId = userCultureId }, //Produce dynamically
                     userLocalizationKeyLambda, //localization => new { localization.LocalizableId, localization.CultureId },
                     userGroupResultSelector)
                .SelectMany(
                    userSelectDefaultIfEmpty,
                    userSelectResultSelector)
                .Select(entitySelectLambda)
                ;

            // var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(finalResult.Provider);
            // var modelGenerator = (QueryModelGenerator)QueryModelGeneratorField.GetValue(queryCompiler);
            // var queryModel = modelGenerator.ParseQuery(finalResult.Expression);

            var data = finalResult.ToList();
            throw new NotImplementedException();
        }

        private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");

        private static readonly FieldInfo QueryModelGeneratorField = QueryCompilerTypeInfo.DeclaredFields.First(x => x.Name == "_queryModelGenerator");
    }



    internal static class CachedReflectionInfo
    {
        // private static readonly MethodInfo _includeMethodInfo
        //     = typeof(IncludeCompiler).GetTypeInfo()
        //         .GetDeclaredMethod("_Include");

        private static Type LocalizationJoinKeyType;

        public static Type GetLocalizationJoinKeyType()
        {
            return (LocalizationJoinKeyType ?? 
                   (LocalizationJoinKeyType = new { LocalizableId = Guid.Empty, CultureId = Guid.Empty }.GetType()));
        }

        private static Dictionary<Type, ConstructorInfo> JoinConstructorInfo = new Dictionary<Type, ConstructorInfo>();

        public static ConstructorInfo GetJoinConstructorInfo(Type TKey) =>
            JoinConstructorInfo.TryGetValue(TKey, out var constructorInfo) ? constructorInfo : 
            JoinConstructorInfo.TryAdd(TKey, TKey.GetConstructors().First()) ? JoinConstructorInfo[TKey] : 
            null;

        private static Dictionary<Type, IEnumerable<MemberInfo>> JoinConstructorMemberInfos = new Dictionary<Type, IEnumerable<MemberInfo>>();

        public static IEnumerable<MemberInfo> GetJoinConstructorMemberInfos(Type TKey) =>
            JoinConstructorMemberInfos.TryGetValue(TKey, out var constructorMemberInfos) ? constructorMemberInfos : 
            JoinConstructorMemberInfos.TryAdd(TKey, TKey.GetProperties()) ? JoinConstructorMemberInfos[TKey] : 
            null;
    }
}
