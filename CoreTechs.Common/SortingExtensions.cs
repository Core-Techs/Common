// Thanks to Marc Gravell for some of the methods (http://stackoverflow.com/a/233505/64334)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CoreTechs.Common
{
    public static class SortingExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, params string[] sortExpressions)
        {
            return source.OrderBy(sortExpressions.Select(SortDescriptor.Parse));
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<SortDescriptor> sorts)
        {
            if (sorts == null) throw new ArgumentNullException("sorts");

            IOrderedQueryable<T> sorted = null;
            var i = 0;
            foreach (var sort in sorts)
            {
                sorted = i == 0
                    ? source.OrderBy(sort.Property, sort.Descending)
                    : sorted.ThenBy(sort.Property, sort.Descending);

                i++;
            }

            return sorted ?? source.OrderBy(x => 0);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property, bool desc)
        {
            return desc ? source.OrderByDescending(property) : source.OrderBy(property);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderBy");
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "OrderByDescending");
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property, bool desc)
        {
            return desc ? source.ThenByDescending(property) : source.ThenBy(property);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenBy");
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder(source, property, "ThenByDescending");
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });

            return (IOrderedQueryable<T>)result;
        }
    }

    public class SortDescriptor
    {
        public string Property { get; set; }
        public bool Descending { get; set; }

        public SortDescriptor()
        {
            // for serializers
        }

        public SortDescriptor(string property, bool descending = false)
        {
            Property = property;
            Descending = descending;
        }

        // parses strings like "Name DESC" or "Whatever ASC"
        // ascending is the default
        public static SortDescriptor Parse(string s)
        {
            var parts = s.Split().Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var property = parts.FirstOrDefault();
            var desc = parts.Skip(1).Take(1).Any(p => p.StartsWith("d", StringComparison.OrdinalIgnoreCase));
            return new SortDescriptor(property, desc);
        }
    }
}