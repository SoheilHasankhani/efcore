namespace LocalizationTest.Data.EntityFramework.Query.ExpressionVisitors.Internal
{
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

    public class SoheilNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
    {
        // extend this to add group joins for localization
        public SoheilNavigationRewritingExpressionVisitor(EntityQueryModelVisitor queryModelVisitor)
            : base(queryModelVisitor)
        {
        }

        public SoheilNavigationRewritingExpressionVisitor(EntityQueryModelVisitor queryModelVisitor, bool navigationExpansionSubquery)
            : base(queryModelVisitor, navigationExpansionSubquery)
        {
        }
    }
}