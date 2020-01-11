namespace LocalizationTest.Data.EntityFramework.Configurations.Basic
{
    using System;
    using System.Collections.Generic;

    using LocalizationTest.Data.EntityFramework.Entities.Basic;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasData(
                new List<Person>
                {
                    new Person()
                    {
                        Id = new Guid("C96C5020-31F9-4DCA-A7C9-1CB55722E4B2"),
                        FirstName = "Soheil",
                        LastName = "Hasankhani"
                    }
                });
        }
    }
}
