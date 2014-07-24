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

        public static PropertyInfo[] GetAllProperties<T>()
        {
            return GetAllProperties(typeof (T));
        }

        public static PropertyInfo[] GetAllProperties(Type type)
        {
            var props = CacheProperties(type).Values;
            return props.ToArray();
        }

        public static PropertyInfo[] GetAllProperties(this object source)
        {
            return GetAllProperties(source.GetType());
        }

        public static PropertyInfo FindProperty(this object source, string name)
        {
            if (source == null) throw new ArgumentNullException("source");

            var props = CacheProperties(source.GetType());
            return props.ContainsKey(name) ? props[name] : null;
        }

        private static Dictionary<string, PropertyInfo> CacheProperties(Type type)
        {
            return PropertyCache.GetOrAdd(type, k => type.GetRuntimeProperties().ToDictionary(p => p.Name, p => p));
            
        }

        public static object GetPropertyValue(this object source, string name)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException("name");

            var key = new { Type = source.GetType(), name };

            var getter = GetterCache.GetOrAdd(key, k =>
            {
                var prop = key.Type.FindProperty(name);

                if (prop == null)
                {
                    throw new ArgumentOutOfRangeException("name", "Property not found: " + name);
                }

                var instance = Expression.Parameter(typeof(object), "instance");

                var instanceCast = (!prop.DeclaringType.IsValueType)
                    ? Expression.TypeAs(instance, prop.DeclaringType)
                    : Expression.Convert(instance, prop.DeclaringType);

                var call = Expression.Call(instanceCast, prop.GetMethod);

                var lambda = Expression.Lambda<Func<object, object>>(
                    Expression.TypeAs(call, typeof (object)), instance);
                
                return lambda.Compile();

            });

            return getter(source);
        }

        public static TProp GetPropertyValue<TProp>(this object instance, string name)
        {
            return (TProp)instance.GetPropertyValue(name);
        }

        // todo setters
    }
}
