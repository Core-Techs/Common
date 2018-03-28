using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CoreTechs.Common.Text
{
    public class CsvWriter
    {
        readonly TextWriter _writer;
        readonly char _delim;
        readonly char _tq;
        bool _newRec = true;
        public CsvWriter(TextWriter writer, char delimiter = ',', char textQualifier = '"')
        {
            _writer = writer;
            _delim = delimiter;
            _tq = textQualifier;
        }

        public CsvWriter AddFields(IEnumerable data)
        {
            if (data is string)
                data = new[] { data };

            foreach (var item in data)
                AddField(item);

            return this;
        }

        public CsvWriter AddFields(params object[] data)
        {
            return AddFields(data.AsEnumerable());
        }

        public CsvWriter AddRecord(IEnumerable data)
        {
            if (data is string)
                data = new[] { data };

            return AddFields(data).EndRecord();
        }

        public CsvWriter AddRecord(ICsvWritable data)
        {
            return AddRecord(data.GetCsvFields());
        }

        public CsvWriter AddRecord(params object[] data)
        {
            return AddFields(data.AsEnumerable()).EndRecord();
        }

        public CsvWriter AddField(object obj)
        {
            var s = obj as string ?? obj?.ToString() ?? "";

            bool hasTq;
            var txt = (hasTq = s.Any(c => c == _tq))
                      || char.IsWhiteSpace(s.FirstOrDefault())
                      || char.IsWhiteSpace(s.LastOrDefault())
                      || s.Any(c => c == _delim || c == '\r' || c == '\n');

            if (hasTq)
                s = s.Replace(_tq.ToString(CultureInfo.InvariantCulture),
                    _tq.ToString(CultureInfo.InvariantCulture) + _tq);

            if (!_newRec)
                _writer.Write(_delim);

            if (txt)
                _writer.Write(_tq);

            _writer.Write(s);

            if (txt)
                _writer.Write(_tq);

            _newRec = false;
            return this;
        }

        public CsvWriter EndRecord()
        {
            _writer.Write(Environment.NewLine);
            _newRec = true;
            return this;
        }

        public static void Write<T>(IEnumerable<T> data, TextWriter writer, char delimiter = ',', char textQualifier = '"')
            where T : ICsvWritable
        {
            var csv = new CsvWriter(writer, delimiter, textQualifier);
            var it = data.GetEnumerator();
            if (it.MoveNext())
                csv.AddRecord(it.Current.GetCsvHeadings())
                    .AddRecord(it.Current.GetCsvFields());

            while (it.MoveNext())
                csv.AddRecord(it.Current.GetCsvFields());
        }
    }
}