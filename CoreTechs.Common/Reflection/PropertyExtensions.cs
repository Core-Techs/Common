using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreTechs.Common.Reflection
{
    public static class PropertyExtensions
    {
        private static readonly ConcurrentDictionary<object, Dictionary<string, PropertyInfo>> PropertyCache =
            new ConcurrentDictionary<object, Dictionary<string, PropertyInfo>>();

        private static readonly ConcurrentDictionary<object, Func<object, object>> GetterCache =
            new ConcurrentDictionary<object, Func<object, object>>();

        private static readonly ConcurrentDictionary<object, Action<object, object>> SetterCache =
            new ConcurrentDictionary<object, Action<object, object>>();

        public static PropertyInfo[] GetAllProperties<T>()
        {
            return GetAllProperties(typeof(T));
        }

        public static PropertyInfo[] GetAllProperties(Type type)
        {
            var props = GetCachedProperties(type).Values;
            return props.ToArray();
        }

        public static PropertyInfo[] GetAllProperties(this object source)
        {
            var type = source.GetType();
            return GetAllProperties(type);
        }

        public static PropertyInfo FindProperty(this object source, string name)
        {
            if (source == null) throw new ArgumentNullException("source");

            var type = source.GetType();
            var props = GetCachedProperties(type);
            return props.ContainsKey(name) ? props[name] : null;
        }

        private static Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
        {
            return PropertyCache.GetOrAdd(type, k => type.GetRuntimeProperties().ToDictionary(p => p.Name, p => p));
        }

        public static object GetPropertyValue(this object source, string name)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");

            var prop = source.FindProperty(name);

            if (prop == null)
            {
                throw new ArgumentOutOfRangeException("name", "Property not found: " + name);
            }

            var key = new { Type = source.GetType(), name };

            var getter = GetterCache.GetOrAdd(key, k =>
            {

                var instance = Expression.Parameter(typeof(object), "instance");

                var instanceCast = !prop.DeclaringType.IsValueType
                    ? Expression.TypeAs(instance, prop.DeclaringType)
                    : Expression.Convert(instance, prop.DeclaringType);

                var methodCallExpression = Expression.Call(instanceCast, prop.GetMethod);

                var lambda = Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(methodCallExpression, typeof(object)), instance);

                return lambda.Compile();

            });

            return getter(source);
        }

        public static TProp GetPropertyValue<TProp>(this object instance, string name)
        {
            return (TProp)instance.GetPropertyValue(name);
        }


        public static void SetPropertyValue(this object source, string name, object value)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            var prop = source.FindProperty(name);

            if (prop == null)
            {
                throw new ArgumentOutOfRangeException("name", "Property not found: " + name);
            }

            if (!prop.PropertyType.IsInstanceOfType(value))
            {
                throw new ArgumentException(
                    string.Format("value of type {0} cannot be assigned to property of type {1}",
                        value.GetType().Name, prop.PropertyType.Name));
            }

            var key = new { Type = source.GetType(), name };
            var setter = SetterCache.GetOrAdd(key, k =>
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var valueExp = Expression.Parameter(typeof(object), "value");

                // value as T is slightly faster than (T)value, so if it's not a value type, use that
                var instanceCast = (!prop.DeclaringType.IsValueType) ? Expression.TypeAs(instance, prop.DeclaringType) : Expression.Convert(instance, prop.DeclaringType);

                var valueCast = (!prop.PropertyType.IsValueType) ? Expression.TypeAs(valueExp, prop.PropertyType) : Expression.Convert(valueExp, prop.PropertyType);

                var lambda = Expression.Lambda<Action<object, object>>(
                    Expression.Call(instanceCast, prop.GetSetMethod(), valueCast), new[] {instance, valueExp});

                return lambda.Compile();

            });

            setter(source, value);
        }

    }


}
