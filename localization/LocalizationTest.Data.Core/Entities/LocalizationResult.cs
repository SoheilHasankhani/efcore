namespace LocalizationTest.Data.Core.Entities
{
    using LocalizationTest.Data.Core.Abstraction;

    public class LocalizationResult<TKey, TEntity, TLocalization> : ILocalizationResult<TKey, TEntity, TLocalization>
        where TEntity : IBaseEntity<TKey>
        where TLocalization : ILocalization<ILocalizable<TKey>>
    {
        public TEntity Entity { get; set; }

        public TLocalization BusinessLocalization { get; set; }

        public TLocalization UserLocalization { get; set; }
    }
}
