namespace LocalizationTest.Data.EntityFramework.Query
{
    using Microsoft.EntityFrameworkCore.Query;

    public class SoheilRelationalQueryModelVisitorFactory : RelationalQueryModelVisitorFactory
    {
        public SoheilRelationalQueryModelVisitorFactory(EntityQueryModelVisitorDependencies dependencies, RelationalQueryModelVisitorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext, EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new SoheilRelationalQueryModelVisitor(
                Dependencies,
                RelationalDependencies,
                (RelationalQueryCompilationContext)queryCompilationContext,
                (SoheilRelationalQueryModelVisitor)parentEntityQueryModelVisitor);
    }
}