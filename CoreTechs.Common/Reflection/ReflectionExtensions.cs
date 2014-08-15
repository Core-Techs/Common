using System;
using System.Linq;
using System.Reflection;

namespace CoreTechs.Common.Reflection
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns all properties found for the type, but as they are retrieved from their own declaring types.
        /// This helps to ensure that the get and set methods can be accessed.
        /// It's recommended to use this method to retrieve a type's properties and then use LINQ for filtering.
        /// </summary>
        public static PropertyInfo[] GetPropertiesAsDeclared(this Type type)
        {
            return Memoizer.InternalInstance.Value.Get(type, () => type.GetRuntimeProperties()
                .Select(p => p.DeclaringType.GetRuntimeProperty(p.Name))
                .ToArray());
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
