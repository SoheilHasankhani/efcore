// ReSharper disable StyleCop.SA1402

namespace LocalizationTest.Data.Core.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    using LocalizationTest.Data.Core.Abstraction;

    public class Localizable<TKey, TLocalization> : BaseEntity<TKey>, ILocalizable<TKey, TLocalization>
        where TLocalization : ILocalization<TKey>
    {
        [NotMapped]
        public TLocalization Localization { get; set; }

        public ICollection<TLocalization> Localizations { get; set; }
    }
}
