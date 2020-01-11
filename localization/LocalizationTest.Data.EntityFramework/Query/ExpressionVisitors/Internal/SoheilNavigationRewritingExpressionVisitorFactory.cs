namespace LocalizationTest.Data.EntityFramework.Query.ExpressionVisitors.Internal
{
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

    public class SoheilNavigationRewritingExpressionVisitorFactory : NavigationRewritingExpressionVisitorFactory
    {
        public override NavigationRewritingExpressionVisitor Create(EntityQueryModelVisitor queryModelVisitor)
            => new SoheilNavigationRewritingExpressionVisitor(queryModelVisitor);
    }
}