using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CoreTechs.Common.Reflection;

namespace CoreTechs.Common
{
    public class EnumConverter : System.ComponentModel.EnumConverter
    {
        private static readonly Type[] GoodTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(string)
        };

        public static void Register()
        {
            ConversionExtensions.RegisterTypeConverter(typeof(EnumConverter), typeof(Enum));
        }

        public EnumConverter(Type type)
            : base(type)
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return GoodTypes.Contains(sourceType) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = null;
            if (value != null)
                str = value as string ?? value.ToString();

            return base.ConvertFrom(context, culture, str);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsNullable())
                return CanConvertTo(context, Nullable.GetUnderlyingType(destinationType));

            return GoodTypes.Contains(destinationType) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType.IsNullable() && value != null)
                return ConvertTo(context, culture, value, Nullable.GetUnderlyingType(destinationType));

            if (GoodTypes.Contains(destinationType))
                return Convert.ChangeType(value, destinationType);

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

