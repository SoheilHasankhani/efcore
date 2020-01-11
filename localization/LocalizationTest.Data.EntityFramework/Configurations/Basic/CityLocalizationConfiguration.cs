// ReSharper disable StringLiteralTypo
namespace LocalizationTest.Data.EntityFramework.Configurations.Basic
{
    using System;
    using System.Collections.Generic;

    using LocalizationTest.Data.EntityFramework.Entities.Basic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class CityLocalizationConfiguration : IEntityTypeConfiguration<CityLocalization>
    {
        public void Configure(EntityTypeBuilder<CityLocalization> builder)
        {
            builder.HasData(
                new List<CityLocalization>
                    {
                        new CityLocalization()
                            {
                                Id = new Guid("E7000DA7-A93F-46F8-843E-C7840F7C1FFC"),
                                Title = "Tehran",
                                Description = "Tehran Description",
                                CultureId = new Guid("CA8A9C53-31C1-458D-9844-504A33309E31"),
                                LocalizableId = new Guid("E8FD560C-5C5E-45D4-B93A-394A26909CDF")
                            },
                        new CityLocalization()
                            {
                                Id = new Guid("49E24A56-9A47-4684-9A15-A5891E957C22"),
                                Title = "تهران",
                                Description = "توضیحات تهران",
                                CultureId = new Guid("0C859760-624E-459D-9DDD-FA8B0BCCA02B"),
                                LocalizableId = new Guid("E8FD560C-5C5E-45D4-B93A-394A26909CDF")
                            }
                    });
        }
    }
}