using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreTechs.Common
{
    public static class StringExtensions
    {

        /// <summary>
        /// Joins string representation of items in source enumerable with a seperator.
        /// </summary>
        public static string Join(this IEnumerable source, string separator = ", ")
        {
            if (source == null) throw new ArgumentNullException("source");
            if (separator == null) throw new ArgumentNullException("separator");

            return source.Join(_ => separator);
        }

        /// <summary>
        /// Joins string representation of items in source enumerable with a seperator.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <param name="separatorFunc">A function that takes the 0 based index of the separator and returns a seperator string.</param>
        /// <returns>The joined string.</returns>
        public static string Join(this IEnumerable source, Func<int, string> separatorFunc)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (separatorFunc == null) throw new ArgumentNullException("separatorFunc");

            var it = source.GetEnumerator();
            if (!it.MoveNext()) return "";
            var result = new StringBuilder(it.Current.ToString());

            var i = 0;
            while (it.MoveNext())
                result.Append(separatorFunc(i++) + it.Current);

            return result.ToString();
        }

        /// <summary>
        /// Replace parts of the string
        /// </summary>
        public static string Replace(this string s, string oldValue, string newValue, StringComparison stringComparison)
        {
            return s.Replace(oldValue, old => newValue, stringComparison);
        }

        /// <summary>
        /// Replace parts of the string.
        /// </summary>
        /// <param name="s">The source string</param>
        /// <param name="oldValue">The old value to replace</param>
        /// <param name="newValueFactory">A function that takes the old value being replaced and returns the new value.</param>
        /// <param name="stringComparison">Case sensitivity when searching for the old value to replace.</param>
        /// <returns>The string with replacements made.</returns>
        public static string Replace(this string s, string oldValue, Func<string, string> newValueFactory, StringComparison stringComparison)
        {
            var idx = 0;
            while (true)
            {
                idx = s.IndexOf(oldValue, idx, stringComparison);
                if (idx == -1)
                    break;

                var newValue = newValueFactory(s.Substring(idx, oldValue.Length));
                s = s.Remove(idx, oldValue.Length).Insert(idx, newValue);
                idx += newValue.Length;
            }

            return s;
        }

        public static bool Contains(this string s, string value, StringComparison stringComparison)
        {
            return s.IndexOf(value, stringComparison) != -1;
        }

        public static bool ContainsAll(this string s, params string[] others)
        {
            return s.ContainsAll(StringComparison.CurrentCulture, others);
        }

        public static bool ContainsAll(this string s, StringComparison comparisonType, params string[] others)
        {
            return others.All(o => s.Contains(o, comparisonType));
        }

        public static bool ContainsAny(this string s, params string[] others)
        {
            return s.ContainsAny(StringComparison.CurrentCulture, others);
        }

        public static bool ContainsAny(this string s, StringComparison comparisonType, params string[] others)
        {
            return others.Any(o => s.Contains(o, comparisonType));
        }

        public static bool ContainsMin(this string s, int min, params string[] others)
        {
            return s.ContainsMin(min, StringComparison.CurrentCulture, others);
        }

        public static bool ContainsMin(this string s, int min, StringComparison comparisonType, params string[] others)
        {
            return others.Where(o => s.Contains(o, comparisonType)).Take(min).Count() == min;
        }

        public static bool ContainsMax(this string s, int max, params string[] others)
        {
            return s.ContainsMax(max, StringComparison.CurrentCulture, others);
        }

        public static bool ContainsMax(this string s, int max, StringComparison comparisonType, params string[] others)
        {
            return others.Where(o => s.Contains(o, comparisonType)).Take(max + 1).Count() <= max;
        }

        /// <summary>
        /// Gets substring by index and length.
        /// Doesn't fail if the index and length do not refer to a location within the string.
        /// </summary>
        public static string SafeSubstring(this string s, int startIndex, int length)
        {
            if (s == null) throw new ArgumentNullException("s");

            // originally implemented as:
            //   return string.Concat(s.Skip(startIndex).Take(length));
            // but the following code is ~123x faster

            if (s.Length < startIndex)
                return "";

            if (startIndex < 0)
                startIndex = 0;

            if (length < 0)
                length = 0;

            unchecked
            {
                var totalLength = startIndex + length;

                // overflow?
                if (totalLength < 0)
                    totalLength = int.MaxValue;

                if (totalLength > s.Length)
                    length = s.Length - startIndex;
            }

            return s.Substring(startIndex, length);
        }

        public static string Reverse(this string s)
        {
            return s.ToTextElements().Reverse().StringConcat();
        }

        public static IEnumerable<string> ToTextElements(this string source)
        {
            var e = StringInfo.GetTextElementEnumerator(source);
            while (e.MoveNext())
                yield return e.GetTextElement();
        }

        /// <summary>
        /// Splits a string where each character satisfies the predicate.
        /// </summary>
        public static IEnumerable<string> SplitWhere(this string s, Func<char, bool> predicate, bool returnSplitChars = false)
        {
            if (s == null) throw new ArgumentNullException("s");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var curr = new StringBuilder();
            foreach (var c in s)
            {
                if (predicate(c))
                {
                    if (curr.Length > 0)
                    {
                        yield return curr.ToString();
                        curr.Clear();
                    }

                    if (returnSplitChars)
                        yield return c.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    curr.Append(c);
                }
            }

            if (curr.Length > 0)
                yield return curr.ToString();
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static T Parse<T>(this string s, Func<string, T> parser)
        {
            return parser(s);
        }

        public static T ParseOrDefault<T>(this string s, Func<string, T> parser, T @default = default(T))
        {
            try
            {
                return s.Parse(parser);
            }
            catch
            {
                return @default;
            }
        }

        public static StringReader ToStringReader(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");
            return new StringReader(s);
        }

        public static string StringConcat<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            return string.Concat(source);
        }

        public static IEnumerable<string> FormatMany<T>(this IEnumerable<T> source, string format,
            IFormatProvider formatProvider = null) where T : IFormattable
        {
            return source.Select(x => x.ToString(format, formatProvider));
        }

        public static string StringConcat<T>(this IEnumerable<T> source, string format, IFormatProvider formatProvider = null) where T: IFormattable
        {
            if (source == null) throw new ArgumentNullException("source");

            return source.FormatMany(format, formatProvider).StringConcat();
        }

        public static IEnumerable<string> ReadLines(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            while (true )
            {
                var line = reader.ReadLine();
                if (line == null)
                    yield break;

                yield return line;
            }
        }

        public static IEnumerable<string> ReadLines(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");
            return s.ToStringReader().ReadLines();
        }

        public static IEnumerable<char> EnumerateCharacters(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            while (true)
            {
                var c = reader.Read();
                if (c == -1)
                    yield break;

                yield return (char)c;
            }
        }

        public static string RegexReplace(this string input, string pattern, string replacement, RegexOptions options = RegexOptions.None)
        {
            return Regex.Replace(input, pattern, replacement, options);
        }
    }
}