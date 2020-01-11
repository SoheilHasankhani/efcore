namespace LocalizationTest.Data.EntityFramework.Entities.Basic
{
    using LocalizationTest.Data.EntityFramework.Base.Entities;

    public class Person : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
