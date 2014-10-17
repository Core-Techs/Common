using System;
using System.ComponentModel;
using System.Linq;

namespace CoreTechs.Common
{
    public static class ConversionExtensions
    {
        public static void RegisterAllCustomTypeConverters()
        {
            DateTimeOffsetConverter.Register();
            EnumConverter.Register();
        }

        public static T ConvertTo<T>(this object obj)
        {
            var converted = obj.ConvertTo(typeof (T));
            return (T) converted;
        }

        public static object ConvertTo(this object obj, Type targetType)
        {
            if (targetType.IsInstanceOfType(obj))
                return obj;

            if (obj == DBNull.Value)
                obj = null;

            var targetConv = TypeDescriptor.GetConverter(targetType);
            TypeConverter sourceConv = obj != null ? TypeDescriptor.GetConverter(obj.GetType()) : null;

            var attempt1 = Attempt.Get(() => targetConv.ConvertFrom(obj));
            if (attempt1.Succeeded)
                return attempt1.Value;

            Attempt<object> attempt2 = null;
            if (sourceConv != null)
                attempt2 = Attempt.Get(() => sourceConv.ConvertTo(obj, targetType));

            if (attempt2 != null && attempt2.Succeeded)
                return attempt2.Value;

            var attempt3 = Attempt.Get(() => Convert.ChangeType(obj, targetType));
            if (attempt3.Succeeded)
                return attempt3.Value;

            if (targetType == typeof(string) && obj != null)
                return obj.ToString();

            var exceptions = new[] { attempt1, attempt2, attempt3}
                .Where(x => x != null)
                .Select(x => x.Exception).ToArray();

            throw new InvalidCastException("Conversion failed", new AggregateException(exceptions));
        }
    }
}
