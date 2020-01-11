namespace LocalizationTest.Data.EntityFramework.Base.Abstraction
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;

    public interface ILocalizable<TLocalization> : ILocalizable<Guid, TLocalization>
        where TLocalization : Core.Abstraction.ILocalization<Guid>
    {
    }
}