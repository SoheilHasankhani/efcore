namespace LocalizationTest.Data.EntityFramework.Entities.Basic
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;

    public class Bank : IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
    }
}