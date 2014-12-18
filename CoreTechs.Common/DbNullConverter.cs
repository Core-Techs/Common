using System;
using System.ComponentModel;
using System.Globalization;
using CoreTechs.Common.Reflection;

namespace CoreTechs.Common
{
    public class DbNullConverter : TypeConverter
    {
        public static void Register()
        {
            ConversionExtensions.RegisterTypeConverter(typeof (DbNullConverter), typeof (DBNull));
        }

        private static bool CanTypeStoreNull(Type sourceType)
        {
            return sourceType.IsNullable() || !sourceType.IsValueType;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return CanTypeStoreNull(sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return CanTypeStoreNull(destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value == null ? DBNull.Value : base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (CanTypeStoreNull(destinationType))
                return null;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}