using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreTechs.Common.Text
{
    public static class Extensions
    {

        /// <summary>
        /// Parses delimited text.
        /// </summary>
        /// <param name="reader">The source of character data to parse.</param>
        /// <param name="delimiter">The character that separates fields of data.</param>
        /// <param name="textQualifier">The character that surrounds field text that may contain the delimiter or the text qualifier itself. For literal occurrences of this character within the field data, the character should be doubled.</param>
        /// <returns>An enumerable of string arrays.</returns>
        /// <remarks> 
        /// This implementation is simple and performs reasonably well.
        /// For more features or better performance, CsvHelper is recommended:
        /// https://www.nuget.org/packages/CsvHelper/
        /// </remarks>
        public static IEnumerable<string[]> ReadDelimited(this TextReader reader, char delimiter = ',', char textQualifier = '"')
        {
            if (reader == null) throw new ArgumentNullException("reader");

            const int readNothing = -1;
            var data = new LinkedList<CharWrapper>();
            var inTxt = false;
            int read;
            string[] result;

            while ((read = reader.Read()) != readNothing)
            {
                var c = (char)read;
                read = reader.Peek();
                var next = read == readNothing ? (char?)null : (char)read;
                var isDelim = c == delimiter;
                var isQual = c == textQualifier;
                var is2Quals = isQual && next == textQualifier;
                var isNl = c == '\r' || c == '\n';

                // not in quote and reached new line?
                if (!inTxt && isNl)
                {
                    result = PreYieldResult(data);
                    if (ShouldYield(result)) yield return result;
                }

                // not txt qualified AND reached quote?
                else if (!inTxt && isQual)
                {
                    // this field is text qualified
                    inTxt = true;
                }

                // reached 2 qualifiers within a qualified field?
                else if (inTxt && is2Quals)
                {
                    // treat the qualifier as a literal character within the field text
                    data.AddLast(new CharWrapper(c, true));
                    reader.Skip();
                }

                // reached qualified field terminator?
                else if (inTxt & isQual)
                {
                    // this text qualified field has come to an end
                    inTxt = false;
                }

                // reached any character with text qualified field?
                else if (inTxt)
                {
                    // treat as a literal field character 
                    data.AddLast(new CharWrapper(c, true));
                }

                // reached a field delimiter
                else if (isDelim)
                {
                    // record delim
                    data.AddLast(new FieldTerminator(c));
                }

                else
                {
                    // character is part of current field
                    data.AddLast(new CharWrapper(c, false));
                }
            }

            result = PreYieldResult(data);
            if (ShouldYield(result)) yield return result;
        }

        /// <summary>
        /// Parses delimited text. The first row of data should be a header record containing the names of the fields in the following records.
        /// </summary>
        /// <param name="reader">The source of character data to parse.</param>
        /// <param name="delimiter">The character that separates fields of data.</param>
        /// <param name="textQualifier">The character that surrounds field text that may contain the delimiter or the text qualifier itself. For literal occurrences of this character within the field data, the character should be doubled.</param>
        /// <returns>An enumerable of <see cref="Record"/> objects.</returns>
        /// <remarks> 
        /// This implementation is simple and performs reasonably well.
        /// For more features or better performance, CsvHelper is recommended:
        /// https://www.nuget.org/packages/CsvHelper/
        /// </remarks>
        public static IEnumerable<Record> ReadDelimitedWithHeader(this TextReader reader,
           StringComparer fieldKeyComparer = null,
           char delimiter = ',',
           char textQualifier = '"')
        {
            if (reader == null) throw new ArgumentNullException("reader");

            var it = reader.ReadDelimited(delimiter, textQualifier).GetEnumerator();
            if (!it.MoveNext())
                yield break;

            var header = it.Current;

            while (it.MoveNext())
                yield return new Record(header, it.Current, fieldKeyComparer ?? StringComparer.OrdinalIgnoreCase);
        }

        private static string[] PreYieldResult(ICollection<CharWrapper> data)
        {
            // create string array
            var rawFields = data.SplitWhere(x => x is FieldTerminator);
            var fields = new List<string>();

            foreach (var raw in rawFields)
            {
                // trim unqualified spaces from left
                CharWrapper c;
                while (raw.Count > 0 && !(c = raw.First.Value).Qualified && char.IsWhiteSpace(c.Character))
                    raw.RemoveFirst();

                // trim unqualified spaces from right
                while (raw.Count > 0 && !(c = raw.Last.Value).Qualified && char.IsWhiteSpace(c.Character))
                    raw.RemoveLast();

                fields.Add(raw.Select(x => x.Character).StringConcat());
            }

            data.Clear();
            return fields.ToArray();
        }

        private static bool ShouldYield(IList<string> result)
        {
            if (result.Count == 0)
                return false;

            if (result.Count > 1)
                return true;

            return !string.IsNullOrWhiteSpace(result[0]);
        }

        private static void Skip(this TextReader r, int n = 1)
        {
            for (var i = 0; i < n; i++)
                r.Read();
        }


        private class CharWrapper
        {
            public char Character { get; private set; }
            public bool Qualified { get; private set; }

            public CharWrapper(char c, bool qualified)
            {
                Character = c;
                Qualified = qualified;
            }
        }

        private class FieldTerminator : CharWrapper
        {
            public FieldTerminator(char c) : base(c, false)
            {
            }
        }
    }


}
