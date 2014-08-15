using System;
using System.ComponentModel;
using CoreTechs.Common.Reflection;

namespace CoreTechs.Common
{
    public static class ConversionExtensions
    {
        public static T ConvertTo<T>(this object obj)
        {
            return (T) obj.ConvertTo(typeof (T));
        }

        public static object ConvertTo(this object obj, Type type)
        {
            if (type.IsInstanceOfType(obj))
                return obj;

            if (obj == null && (!type.IsValueType || type.IsNullable()))
                return null;

            var converter = TypeDescriptor.GetConverter(type);
            
            Exception changeTypeEx, convertEx;
            object result;

            try
            {
                result = converter.ConvertFrom(obj);
                return result;
            }
            catch(Exception ex)
            {
                convertEx = ex;
                // TypeDescriptor converter didn't work
            }

            if (type.IsNullable())
                type = Nullable.GetUnderlyingType(type);

            try
            {
                result = Convert.ChangeType(obj, type);
                return result;
            }
            catch(Exception ex)
            {
                changeTypeEx = ex;
                // Convert.ChangeType didn't work
            }

            if (type == typeof (string))
                return obj.ToString();

            throw new InvalidCastException("Conversion failed", new AggregateException(changeTypeEx, convertEx));

        }
    }

}