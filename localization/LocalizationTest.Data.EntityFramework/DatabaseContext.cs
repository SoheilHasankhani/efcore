namespace LocalizationTest.Data.EntityFramework
{
    using LocalizationTest.Data.EntityFramework.Configurations.Basic;
    using LocalizationTest.Data.EntityFramework.Entities.Basic;
    using LocalizationTest.Data.EntityFramework.Query;
    using LocalizationTest.Data.EntityFramework.Query.ExpressionVisitors.Internal;
    using LocalizationTest.Data.EntityFramework.Query.Internal;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<CityLocalization> CityLocalizations { get; set; }

        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CityConfiguration())
                .ApplyConfiguration(new CityLocalizationConfiguration())
                .ApplyConfiguration(new AddressConfiguration())
                .ApplyConfiguration(new PersonConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Register new IncludeLocalization method
            optionsBuilder.ReplaceService<INodeTypeProviderFactory, SoheilMethodInfoBasedNodeTypeRegistryFactory>();

            // To provide customized RelationalQueryModelVisitor which already process IncludeLocalization
            optionsBuilder.ReplaceService<IEntityQueryModelVisitorFactory, SoheilRelationalQueryModelVisitorFactory>();
            
            // To provide client handler for Localization result (Handler is not correct answer, it should be used for kind of transformations which would be executed once per database request
            // in case of multiple IncludeLocalization it would not work properly)
            // optionsBuilder.ReplaceService<IResultOperatorHandler, SoheilResultOperatorHandler>();




            // optionsBuilder.ReplaceService<IQueryModelGenerator, SoheilQueryModelGenerator>();
            // optionsBuilder.ReplaceService<INavigationRewritingExpressionVisitorFactory, SoheilNavigationRewritingExpressionVisitorFactory>();
        }
    }
}
