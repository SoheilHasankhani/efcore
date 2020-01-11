namespace LocalizationTest.Data.EntityFramework.Entities.Basic
{
    using System.ComponentModel.DataAnnotations.Schema;

    using LocalizationTest.Data.EntityFramework.Base.Entities;

    [Table(nameof(City), Schema = nameof(Basic))]
    public class City : Localizable<CityLocalization>
    {
        public string PhoneCode { get; set; }
    }
}