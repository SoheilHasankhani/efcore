namespace LocalizationTest.Data.Core.Abstraction
{
    public interface IBaseEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
