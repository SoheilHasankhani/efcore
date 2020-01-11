namespace LocalizationTest.Data.EntityFramework.Query.Internal
{
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;

    using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

    public class SoheilQueryModelGenerator : QueryModelGenerator
    {
        public SoheilQueryModelGenerator(
            INodeTypeProviderFactory nodeTypeProviderFactory,
            IEvaluatableExpressionFilter evaluatableExpressionFilter,
            ICurrentDbContext currentDbContext)
            : base(nodeTypeProviderFactory, evaluatableExpressionFilter, currentDbContext)
        {
        }
    }
}