namespace LocalizationTest.Data.EntityFramework.Models
{
    using System;

    using LocalizationTest.Data.Core.Abstraction;

    public class LocalizationResult<TEntity, TLocalization>
        where TLocalization : class, ILocalization<Guid>
    {
        public LocalizationResult(
            TEntity entity,
            TLocalization businessLocalization,
            TLocalization userLocalization)
        {
            Entity = entity;
            BusinessLocalization = businessLocalization;
            UserLocalization = userLocalization;
        }

        // public LocalizationResult(TEntity entity, TLocalization businessLocalization)
        //     : this(entity, businessLocalization, null)
        // {
        // }

        public TLocalization BusinessLocalization { get; set; }

        public TEntity Entity { get; set; }

        public TLocalization UserLocalization { get; set; }
    }
}