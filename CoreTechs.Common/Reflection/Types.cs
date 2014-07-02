using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreTechs.Common.Reflection
{
    public static class Types
    {
        public static IEnumerable<Type> InLoadedAssemblies()
        {
            return InAssemblies(GetLoadedAssemblies());
        }

        private static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static IEnumerable<Type> InAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.DefinedTypes);
        }

        public static IEnumerable<Type> WhereAssignableTo<T>(this IEnumerable<Type> types)
        {
            return types.Where(t => typeof (T).IsAssignableFrom(t));
        }
    }
}
