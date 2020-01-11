namespace LocalizationTest.Data.EntityFramework.Base.Entities
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;
    using LocalizationTest.Data.Core.Entities;

    public class Localizable<TLocalization> : Localizable<Guid, TLocalization>
        where TLocalization : ILocalization<Guid>
    {
    }
}