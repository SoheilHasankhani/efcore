namespace LocalizationTest.Data.EntityFramework.Base.Entities
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;
    using LocalizationTest.Data.Core.Entities;

    public class Localization<TLocalizable> : Localization<Guid, TLocalizable>
        where TLocalizable : ILocalizable<Guid>
    {
    }
}