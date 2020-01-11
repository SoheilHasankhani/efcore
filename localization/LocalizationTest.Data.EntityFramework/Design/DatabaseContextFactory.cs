namespace LocalizationTest.Data.EntityFramework.Design
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseSqlServer(
                "Server=.;Initial Catalog=DataLocalizationTest;Integrated Security=true;Application Name=Localization");

            return new DatabaseContext(builder.Options);
        }
    }
}