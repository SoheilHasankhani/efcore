using System;
using System.Collections.Generic;
using System.Text;

namespace LocalizationTest.Data.EntityFramework.Metadata.Internal
{
    using System.Diagnostics;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    //var navigation = new Navigation(name, propertyIdentity.Property as PropertyInfo, propertyIdentity.Property as FieldInfo, foreignKey);
    public class IgnoredProperty : PropertyBase
    {
        private readonly IEntityType _entityType;

        public IgnoredProperty(string name, PropertyInfo propertyInfo, IEntityType entityType)
            : base(name, propertyInfo, null)
        {
            _entityType = entityType;
        }

        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        public virtual EntityType DeclaringEntityType => _entityType as EntityType;

        public override TypeBase DeclaringType { [DebuggerStepThrough] get => DeclaringEntityType; }

        public override Type ClrType => this.GetIdentifyingMemberInfo()?.GetMemberType() ?? typeof(object);
    }


    internal static class MemberInfoExtensions
    {
        public static Type GetMemberType(this MemberInfo memberInfo) => (memberInfo as PropertyInfo)?.PropertyType ?? ((FieldInfo)memberInfo)?.FieldType;
    }
}
