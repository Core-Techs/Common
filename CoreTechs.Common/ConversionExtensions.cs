using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreTechs.Common.Reflection;

namespace CoreTechs.Common
{
    public static class ConversionExtensions
    {
        public static void RegisterTypeConverter(Type typeConverterType, Type appliedToType)
        {
            if (typeConverterType == null) throw new ArgumentNullException("typeConverter");
            if (appliedToType == null) throw new ArgumentNullException(nameof(appliedToType));

            if (!typeof (TypeConverter).IsAssignableFrom(typeConverterType))
                throw new ArgumentOutOfRangeException(nameof(typeConverterType),
                    string.Format("{1} is not assignable from {0}", typeConverterType.FullName,
                        typeof (TypeConverter).FullName));

            var cacheKey = new
            {
                typeConverterType,
                appliedToType
            };

            // using Cache to ensure one time registration
            Cache.Get(cacheKey,
                () => TypeDescriptor.AddAttributes(appliedToType, new TypeConverterAttribute(typeConverterType)));

        }


        public static void RegisterAllCustomTypeConverters()
        {
            DbNullConverter.Register();
            DateTimeOffsetConverter.Register();
            EnumConverter.Register();
        }

        private static Memoizer Cache => Memoizer.InternalInstance.Value;

        public static T ConvertTo<T>(this object value)
        {
            return (T)value.ConvertTo(typeof(T));
        }

        public static object ConvertTo(this object value, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));

            if (destinationType.IsInstanceOfType(value))
                return value;

            // if the dest type can contain null
            // and the value is null
            // just return null
            var destCanStoreNull = !destinationType.IsValueType || destinationType.IsNullable();
            if (destCanStoreNull && value == null)
                return null;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var sourceType = value.GetType();
            var cacheKey = new
            {
                sourceType,
                destinationType
            };

            var converted = false;
            object convertedValue = null;
            var exs = new List<Exception>();

            var converter = Cache.Get(cacheKey,
                () =>
                {
                    foreach (var func in GetConversionFuncs(sourceType, destinationType))
                    {
                        try
                        {
                            convertedValue = func(value);
                            converted = true;
                            return func;
                        }
                        catch (Exception ex)
                        {
                            exs.Add(ex);
                        }
                    }

                    return null;
                });

            if (converter == null)
                throw new AggregateException(exs);

            return converted ? convertedValue : converter(value);
        }

        private static IEnumerable<Func<object, object>> GetConversionFuncs(Type sourceType, Type destType)
        {
            // if the types are compatible, don't convert
            if (destType.IsAssignableFrom(sourceType))
                yield return x => x;

            foreach (var f in GetCoreConversionFuncs(sourceType, destType))
                yield return f;

            if (sourceType.IsNullable())
                foreach (var f in GetCoreConversionFuncs(Nullable.GetUnderlyingType(sourceType), destType))
                    yield return f;

            if (destType.IsNullable())
                foreach (var f in GetCoreConversionFuncs(sourceType, Nullable.GetUnderlyingType(destType)))
                    yield return f;

            if (sourceType.IsNullable() && destType.IsNullable())
                foreach (var f in GetCoreConversionFuncs(
                    Nullable.GetUnderlyingType(sourceType),
                    Nullable.GetUnderlyingType(destType)))
                    yield return f;

            // last ditch effort:
            // if the destination is string
            // call tostring
            if (destType == typeof(string))
                yield return x => x == null ? null : x.ToString();
        }

        private static IEnumerable<Func<object, object>> GetCoreConversionFuncs(Type sourceType, Type destType)
        {
            yield return x =>
            {
                var converter = TypeDescriptor.GetConverter(destType);
                return converter.ConvertFrom(x);
            };

            yield return x =>
            {
                var converter = TypeDescriptor.GetConverter(sourceType);
                return converter.ConvertTo(x, destType);
            };

            yield return x => Convert.ChangeType(x, destType);

        }
    }
}