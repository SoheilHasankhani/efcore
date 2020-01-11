// ReSharper disable StyleCop.SA1402

namespace LocalizationTest.Data.Core.Entities
{
    using LocalizationTest.Data.Core.Abstraction;

    public class Localization<TKey, TLocalizable> : BaseEntity<TKey>, ILocalization<TKey, TLocalizable>
        where TLocalizable : ILocalizable<TKey>
    {
        public TKey CultureId { get; set; }

        public TLocalizable Localizable { get; set; }

        public TKey LocalizableId { get; set; }
    }
}
