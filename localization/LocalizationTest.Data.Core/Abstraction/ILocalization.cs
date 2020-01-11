namespace LocalizationTest.Data.Core.Abstraction
{
    public interface ILocalization<TKey> : IBaseEntity<TKey>
    {
        TKey CultureId { get; set; }

        TKey LocalizableId { get; set; }
    }

    public interface ILocalization<TKey, TLocalizable> : ILocalization<TKey>
        where TLocalizable : ILocalizable<TKey>

    {
        TLocalizable Localizable { get; set; }
    }
}
