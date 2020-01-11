namespace LocalizationTest.Data.EntityFramework.Entities.Basic
{
    using System.ComponentModel.DataAnnotations.Schema;

    using LocalizationTest.Data.EntityFramework.Base.Entities;

    [Table(nameof(CityLocalization), Schema = nameof(Basic))]
    public class CityLocalization : Localization<City>
    {
        public string Title { get; set; }

        public string Description { get; set; }
    }
}