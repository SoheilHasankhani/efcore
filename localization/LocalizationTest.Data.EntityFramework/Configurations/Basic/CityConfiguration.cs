namespace LocalizationTest.Data.EntityFramework.Configurations.Basic
{
    using System;

    using LocalizationTest.Data.EntityFramework.Entities.Basic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasData(new City { Id = new Guid("E8FD560C-5C5E-45D4-B93A-394A26909CDF"), PhoneCode = "21" });
        }
    }
}