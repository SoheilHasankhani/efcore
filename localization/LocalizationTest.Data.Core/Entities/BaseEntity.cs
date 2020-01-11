namespace LocalizationTest.Data.Core.Entities
{
    using LocalizationTest.Data.Core.Abstraction;

    public class BaseEntity<TKey> : IBaseEntity<TKey>
    {
        public TKey Id { get; set; }
    }
}
