// ReSharper disable All
namespace LocalizationTest.Data.EntityFramework.Query
{
    using LocalizationTest.Data.EntityFramework.Query.Internal;

    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

    using Remotion.Linq;

    public class SoheilRelationalQueryModelVisitor : RelationalQueryModelVisitor
    {
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;

        public SoheilRelationalQueryModelVisitor(
            EntityQueryModelVisitorDependencies dependencies,
            RelationalQueryModelVisitorDependencies relationalDependencies,
            RelationalQueryCompilationContext queryCompilationContext,
            RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(dependencies, relationalDependencies, queryCompilationContext, parentQueryModelVisitor)
        {
            _querySourceTracingExpressionVisitorFactory = dependencies.QuerySourceTracingExpressionVisitorFactory;
        }

        protected override void OnBeforeNavigationRewrite(QueryModel queryModel)
        {
            base.OnBeforeNavigationRewrite(queryModel);

            // Rewrite IncludeLocalizations
            var includeLocalizationCompiler = new IncludeLocalizationCompiler(QueryCompilationContext, _querySourceTracingExpressionVisitorFactory);
            includeLocalizationCompiler.CompileIncludeLocalizations(queryModel, true, false, shouldThrow: false);
        }
    }
}
