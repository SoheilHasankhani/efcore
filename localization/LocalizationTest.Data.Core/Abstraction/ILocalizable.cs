namespace LocalizationTest.Data.Core.Abstraction
{
    using System.Collections.Generic;

    public interface ILocalizable<TKey> : IBaseEntity<TKey>
    {
    }

    public interface ILocalizable<TKey, TLocalization> : ILocalizable<TKey>
        where TLocalization : ILocalization<TKey>
    {
        ICollection<TLocalization> Localizations { get; set; }
    }
}
