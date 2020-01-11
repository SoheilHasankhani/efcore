namespace LocalizationTest.Data.EntityFramework.Configurations.Basic
{
    using System;
    using System.Collections.Generic;

    using LocalizationTest.Data.EntityFramework.Entities.Basic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasData(
                new List<Address>
                    {
                        new Address()
                            {
                                Id = new Guid("A0BA6506-1993-44D3-901B-E199F6800D82"),
                                CityId = new Guid("E8FD560C-5C5E-45D4-B93A-394A26909CDF"),
                                AsDefault = true,
                                ContentAddress = "Test Address",
                                PersonId = new Guid("C96C5020-31F9-4DCA-A7C9-1CB55722E4B2")
                            }
                    });
        }
    }
}