// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILocalizationResult.cs" company="mores">
//   2020
// </copyright>
// <summary>
//   Defines the ILocalizationResult type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LocalizationTest.Data.Core.Abstraction
{
    using System;

    public interface ILocalizationResult<TKey, TEntity, TLocalization>
        where TEntity : IBaseEntity<TKey>
        where TLocalization : ILocalization<ILocalizable<TKey>>
    {
        TEntity Entity { get; set; }

        TLocalization BusinessLocalization { get; set; }

        TLocalization UserLocalization { get; set; }
    }
}
