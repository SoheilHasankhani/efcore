namespace LocalizationTest.Data.EntityFramework.Query
{
    using System.Linq.Expressions;
    using System.Reflection;

    using LocalizationTest.Data.EntityFramework.Query.ResultOperators.Internal;

    using Microsoft.EntityFrameworkCore.Query;

    using Remotion.Linq;
    using Remotion.Linq.Clauses;

    public class SoheilResultOperatorHandler : ResultOperatorHandler
    {
        public SoheilResultOperatorHandler(ResultOperatorHandlerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override Expression HandleResultOperator(EntityQueryModelVisitor entityQueryModelVisitor, ResultOperatorBase resultOperator, QueryModel queryModel)
        {
            if (resultOperator.GetType() == typeof(IncludeLocalizationResultOperator))
            {
                return HandleLocalizationResult(entityQueryModelVisitor, (IncludeLocalizationResultOperator)resultOperator, queryModel);
            }

            return base.HandleResultOperator(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static Expression HandleLocalizationResult(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IncludeLocalizationResultOperator includeLocalizationResultOperator,
            QueryModel queryModel)
        {
            var sequenceType = entityQueryModelVisitor.Expression.Type.GetSequenceType();

            return Expression.Call(TransformLocalizationResultMethodInfo.MakeGenericMethod(sequenceType, sequenceType), entityQueryModelVisitor.Expression);
        }


        public static readonly MethodInfo TransformLocalizationResultMethodInfo = typeof(LocalizableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(LocalizableExtensions.TransformLocalizationResult));
    }
}
