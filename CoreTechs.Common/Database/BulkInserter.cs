using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CoreTechs.Common.Database
{
    /// <summary>
    /// Performs buffered bulk inserts into a sql server table using objects instead of DataRows. :)
    /// </summary>
    public class BulkInserter<T> : IDisposable where T : class
    {
        
        public string[] RemoveColumns { get; set; }
        public IEqualityComparer<string> ColumnNameComparer { get; set; }

        public event EventHandler<BulkInsertEventArgs<T>> PreBulkInsert;
        public void OnPreBulkInsert(BulkInsertEventArgs<T> e)
        {
            var handler = PreBulkInsert;
            handler?.Invoke(this, e);
        }

        public event EventHandler<BulkInsertEventArgs<T>> PostBulkInsert;
        public void OnPostBulkInsert(BulkInsertEventArgs<T> e)
        {
            var handler = PostBulkInsert;
            handler?.Invoke(this, e);
        }

        private const int DefaultBufferSize = 2000;
        private readonly SqlConnection _connection;
        public int BufferSize { get; }

        public int InsertedCount { get; private set; }

        private readonly Lazy<Dictionary<string, Func<T, object>>> _props;

        private readonly Lazy<DataTable> _dt;

        private readonly bool _constructedSqlBulkCopy;
        private readonly SqlBulkCopy _sbc;
        private readonly List<T> _queue = new List<T>();
        public TimeSpan? BulkCopyTimeout { get; set; }
        private readonly SqlTransaction _tran;

        /// <param name="connection">SqlConnection to use for retrieving the schema of sqlBulkCopy.DestinationTableName</param>
        /// <param name="sqlBulkCopy">SqlBulkCopy to use for bulk insert.</param>
        /// <param name="bufferSize">Number of rows to bulk insert at a time. The default is 2000.</param>
        public BulkInserter(SqlConnection connection, SqlBulkCopy sqlBulkCopy, int bufferSize = DefaultBufferSize)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (sqlBulkCopy == null) throw new ArgumentNullException(nameof(sqlBulkCopy));


            BufferSize = bufferSize;
            _connection = connection;
            _sbc = sqlBulkCopy;
            _props = new Lazy<Dictionary<string, Func<T, object>>>(GetPropertyInformation);
            _dt = new Lazy<DataTable>(CreateDataTable);
        }

        /// <param name="connection">SqlConnection to use for retrieving the schema of sqlBulkCopy.DestinationTableName and for bulk insert.</param>
        /// <param name="tableName">The name of the table that rows will be inserted into.</param>
        /// <param name="bufferSize">Number of rows to bulk insert at a time. The default is 2000.</param>
        /// <param name="copyOptions">Options for SqlBulkCopy.</param>
        /// <param name="sqlTransaction">SqlTransaction for SqlBulkCopy</param>
        public BulkInserter(SqlConnection connection, string tableName, int bufferSize = DefaultBufferSize,
                            SqlBulkCopyOptions copyOptions = SqlBulkCopyOptions.Default, SqlTransaction sqlTransaction = null)
            : this(connection, new SqlBulkCopy(connection, copyOptions, sqlTransaction) { DestinationTableName = tableName }, bufferSize)
        {
            _tran = sqlTransaction;
            _constructedSqlBulkCopy = true;
        }

        /// <summary>
        /// Performs buffered bulk insert of enumerable items.
        /// </summary>
        /// <param name="items">The items to be inserted.</param>
        public void Insert(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // get columns that have a matching property
            var cols = _dt.Value.Columns.Cast<DataColumn>()
                .Where(x => _props.Value.ContainsKey(x.ColumnName) )
                .Select(x => new { Column = x, Getter = _props.Value[x.ColumnName] })
                .Where(x => x.Getter != null)
                .ToArray();

            foreach (var buffer in items.Buffer(BufferSize).Select(x => x.ToArray()))
            {
                foreach (var item in buffer)
                {
                    var row = _dt.Value.NewRow();

                    foreach (var col in cols)
                        row[col.Column] = col.Getter(item) ?? DBNull.Value;

                    _dt.Value.Rows.Add(row);
                }

                var bulkInsertEventArgs = new BulkInsertEventArgs<T>(buffer, _dt.Value);
                OnPreBulkInsert(bulkInsertEventArgs);

                if (BulkCopyTimeout.HasValue)
                    _sbc.BulkCopyTimeout = (int)BulkCopyTimeout.Value.TotalSeconds;

                _sbc.WriteToServer(_dt.Value);

                OnPostBulkInsert(bulkInsertEventArgs);

                InsertedCount += _dt.Value.Rows.Count;
                _dt.Value.Clear();
            }
        }

        /// <summary>
        /// Queues a single item for bulk insert. When the queue count reaches the buffer size, bulk insert will happen.
        /// Call Flush() to manually bulk insert the currently queued items.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        public void Insert(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _queue.Add(item);

            if (_queue.Count == BufferSize)
                Flush();
        }

        /// <summary>
        /// Bulk inserts the currently queued items.
        /// </summary>
        public void Flush()
        {
            Insert(_queue);
            _queue.Clear();
        }

        /// <summary>
        /// Sets the InsertedCount property to zero.
        /// </summary>
        public void ResetInsertedCount()
        {
            InsertedCount = 0;
        }

        private Dictionary<string, Func<T, object>> GetPropertyInformation()
        {
            return typeof (T).GetProperties()
                .ToDictionary(x => x.Name, CreatePropertyGetter, ColumnNameComparer ?? StringComparer.OrdinalIgnoreCase);
        }

        private static Func<T, object> CreatePropertyGetter(PropertyInfo propertyInfo)
        {
            if (typeof(T) != propertyInfo.DeclaringType)
                throw new ArgumentException();

            var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var property = Expression.Property(instance, propertyInfo);
            var convert = Expression.TypeAs(property, typeof(object));
            return (Func<T, object>)Expression.Lambda(convert, instance).Compile();
        }

        private DataTable CreateDataTable()
        {
            var dt = new DataTable();
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = _tran;
                cmd.CommandText = $"SELECT * FROM {_sbc.DestinationTableName}";

                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    dt.Load(reader);
                    reader.Close();
                }
            }

            if (RemoveColumns != null)
                foreach (var col in RemoveColumns)
                    dt.Columns.Remove(col);

            return dt;
        }

        public void Dispose()
        {
            if (_constructedSqlBulkCopy)
                using (_sbc) _sbc.Close();
        }
    }

    public class BulkInsertEventArgs<T> : EventArgs
    {
        public T[] Items { get; private set; }
        public DataTable DataTable { get; set; }

        public BulkInsertEventArgs(T[] items, DataTable dataTable)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            Items = items.ToArray();
            DataTable = dataTable;
        }
    }
}