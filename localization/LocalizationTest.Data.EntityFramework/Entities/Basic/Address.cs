namespace LocalizationTest.Data.EntityFramework.Entities.Basic
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using LocalizationTest.Data.EntityFramework.Base.Entities;

    [Table(nameof(Address), Schema = nameof(Basic))]
    public class Address : BaseEntity
    {
        public bool AsDefault { get; set; }

        public City City { get; set; }

        public Guid? CityId { get; set; }

        [MaxLength(1000)]
        public string ContentAddress { get; set; }

        public Guid? PersonId { get; set; }

        public Person Person { get; set; }
    }
}