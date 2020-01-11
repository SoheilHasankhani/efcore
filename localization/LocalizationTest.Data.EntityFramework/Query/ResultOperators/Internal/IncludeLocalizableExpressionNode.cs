namespace LocalizationTest.Data.EntityFramework.Query.ResultOperators.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;

    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.Structure.IntermediateModel;

    public class IncludeLocalizationExpressionNode : ResultOperatorExpressionNodeBase
    {
        protected virtual LambdaExpression NavigationPropertyPathLambda { get; }

        public IncludeLocalizationExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, null, null)
        {
            NavigationPropertyPathLambda = navigationPropertyPathLambda;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            var prm = Expression.Parameter(typeof(object));
            var pathFromQuerySource = Resolve(prm, prm, clauseGenerationContext);

            if (!NavigationPropertyPathLambda.TryGetComplexPropertyAccess(out var propertyPath))
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidIncludeLambdaExpression(
                        nameof(EntityFrameworkQueryableExtensions.Include),
                        NavigationPropertyPathLambda));
            }

            var includeLocalizationResultOperator = new IncludeLocalizationResultOperator(
                propertyPath.Select(p => p.Name),
                pathFromQuerySource);

            clauseGenerationContext.AddContextInfo(this, includeLocalizationResultOperator);

            return includeLocalizationResultOperator;
        }

        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[]
            { LocalizableExtensions.IncludeLocalizationMethodInfo };
    }
}
