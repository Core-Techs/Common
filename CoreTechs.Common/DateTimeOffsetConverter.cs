using System;
using System.ComponentModel;
using System.Globalization;

namespace CoreTechs.Common
{
    /// <summary>
    /// Allows conversion between datetime/datetimeoffset
    /// </summary>
    public class DateTimeOffsetConverter : System.ComponentModel.DateTimeOffsetConverter
    {
        public static void Register()
        {
            var converterType = typeof(DateTimeOffsetConverter);
            Memoizer.InternalInstance.Value.Get(converterType,
                () =>
                {
                    TypeDescriptor.AddAttributes(typeof(DateTimeOffset),
                        new TypeConverterAttribute(converterType));

                    return 0;
                });
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(DateTime))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (dt.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException(
                        "Cannot convert DateTime to DateTimeOffset when the source DateTime object's Kind is Unspecified");

                return new DateTimeOffset(dt);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DateTime))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var dto = (DateTimeOffset)value;

            if (destinationType == typeof(DateTime))
            {
                return dto.LocalDateTime;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}