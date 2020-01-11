namespace LocalizationTest.Console
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LocalizationTest.Data.EntityFramework;
    using LocalizationTest.Data.EntityFramework.Entities.Basic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query.Internal;

    public static class Program
    {
        public static void Main()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseSqlServer("Server=.;Initial Catalog=DataLocalizationTest;Integrated Security=true;Application Name=Localization");
            
            var context = new DatabaseContext(builder.Options);

            Guid businessCultureId = new Guid("F6DE80EB-BC84-4B02-A079-EDE5A304EB11"), 
                 userCultureId = new Guid("EA04E5FB-029D-47B9-9893-2F51DD1995E3");

            var query = context

                    .Addresses.Include(i => i.City).Include(i => i.Person)
                    //.Cities.IncludeLocalization(i => i.Localization)
                ;

            // var query = context.Addresses.Include(address => address.City);
            var queryData = query.ToList();

            //var cityQuery = context.Cities.IncludeLocalization(city => city, businessCultureId, userCultureId);
            //var cityData = cityQuery.ToList();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static bool Test1<TKey>(DatabaseContext context)
        {
            

            //Console.WriteLine(typeof(TKey).ToString());
            var cityParameter = Expression.Parameter(typeof(City), "c");
            var city2Parameter = Expression.Parameter(typeof(City), "c");
            var newExpression = Expression.New(typeof(TKey).GetConstructors().First(), Expression.Property(cityParameter, nameof(City.Id)));
            var newExpression2 = Expression.New(typeof(TKey).GetConstructors().First(), Expression.Property(city2Parameter, nameof(City.Id)));


            Expression<Func<City, object>> lambda = Expression.Lambda<Func<City, object>>(newExpression, cityParameter);
            Expression<Func<City, object>> lambda2 = Expression.Lambda<Func<City, object>>(newExpression2, city2Parameter);


            Expression<Func<City, object>> selector = c => new { Id = c.Id };
            Expression<Func<City, object>> selector2 = c => new { Id = c.Id };

            var query1 = context.Cities.Join(context.Cities, lambda, lambda2, (city, cities) => new {city, cities });
            var query2 = context.Cities.Join(context.Cities, selector, selector2, (city, cities) => new { city, cities });

            query1.ToList();
            query2.ToList();
            // var groupJoin =
            //     context.Cities.GroupJoin(
            //         context.CityLocalizations,
            //         city => new { },
            //         localization => new { },
            //         (city, enumerable) => new { });

            return true;
        }

        public static bool Test2(DatabaseContext context)
        {
            context.Cities
                .GroupJoin(
                    context.CityLocalizations,
                    city => new {LocalizableId = city.Id, CultureId = new Guid("F16C42BA-0E34-4851-9D3C-EF6D388A31F8")},
                    localization => new { LocalizableId = localization.LocalizableId, CultureId = new Guid("EBCB91A5-5AAC-45E8-91D7-709B92DC6B24")},
                    (city, localizations) => new {Entity = city, BusinessLocalization = localizations})
                .GroupJoin(
                    context.CityLocalizations,
                    item => new { LocalizableId = item.Entity.Id, CultureId = new Guid("BADCBD30-F37E-4159-8974-87824CAF825F") },
                    localization => new { LocalizableId = localization.LocalizableId, CultureId = new Guid("88514DE9-556E-4FB7-A862-88F676262CD1") },
                    (item, localizations) => new { Entity = item.Entity, BusinessLocalization = localizations })
                ;

            return false;
        }
    }
}
