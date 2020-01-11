namespace LocalizationTest.Data.EntityFramework.Base.Abstraction
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;

    public interface ILocalization<TLocalizable> : ILocalization<Guid, TLocalizable>
        where TLocalizable : Core.Abstraction.ILocalizable<Guid>
    {
    }
}