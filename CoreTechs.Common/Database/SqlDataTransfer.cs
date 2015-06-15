using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace CoreTechs.Common.Database
{
    /// <summary>
    /// Provides the ability to bulk insert data from a source table (or query) into a destination table.
    /// SQL Server only.
    /// </summary>
    public class SqlDataTransfer
    {
        readonly string _sourceConnString;
        readonly string _destConnString;

        /// <summary>
        /// The max allowed time to complete the data transfer.
        /// </summary>
        public TimeSpan TransferTimeout { get; set; }

        public SqlDataTransfer(string sourceConnectionString, string destinationConnectionString)
        {
            if (sourceConnectionString == null) throw new ArgumentNullException("sourceConnectionString");
            if (destinationConnectionString == null) throw new ArgumentNullException("destinationConnectionString");

            _sourceConnString = sourceConnectionString;
            _destConnString = destinationConnectionString;
        }

        /// <summary>
        /// Transfer data into a destination table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table that will be written to. 
        /// If a query is not provided this will also be the name of the table that data is read from.</param>
        /// <param name="sqlBulkCopyOptions">Options used by <see cref="SqlBulkCopy"/>.</param>
        /// <param name="query">An optional query that will be used as the source of data.</param>
        /// <param name="queryParams">Optional parameters that are used by the source data query.</param>
        /// <param name="sqlTransaction">Optional <see cref="SqlTransaction"/> object that will be used to construct the <see cref="SqlBulkCopy"/>.</param>
        /// <param name="sqlBulkCopyCustomizer">Optional delegate that can customize the <see cref="SqlBulkCopy"/> object.</param>
        /// <param name="smartColumnMapping">
        /// Passing true will cause source columns to be mapped to destination columns by name and computed columns to be ignored.
        /// A value of false will result in ordinal based mapping of all columns. Defaults to true.
        /// </param>
        public void TransferData(string tableName, string query = null, SqlParameter[] queryParams = null, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, SqlTransaction sqlTransaction = null, Action<SqlBulkCopy> sqlBulkCopyCustomizer = null, bool smartColumnMapping = true)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (!tableName.Contains("["))
                tableName = tableName.Split('.').Select(n => "[" + n + "]").Join(".");

            using (var source = new SqlConnection(_sourceConnString))
            using (var dest = new SqlConnection(_destConnString))
            using (var bcp = new SqlBulkCopy(dest, sqlBulkCopyOptions, null))
            using (var sourceCmd = source.CreateCommand())
            using (source.Connect())
            using (dest.Connect())
            {
                query = query.IsNullOrWhiteSpace() ? string.Format("SELECT * FROM {0}", tableName) : query;

                bcp.EnableStreaming = true;
                bcp.DestinationTableName = tableName;
                bcp.BulkCopyTimeout = (int)TransferTimeout.TotalSeconds;

                sourceCmd.CommandType = CommandType.Text;
                sourceCmd.CommandText = query;
                sourceCmd.CommandTimeout = (int)TransferTimeout.TotalSeconds;

                if (queryParams != null && queryParams.Any())
                    sourceCmd.Parameters.AddRange(queryParams);

                using (var reader = sourceCmd.ExecuteReader())
                {
                    if (smartColumnMapping)
                    {
                        var schema = reader.GetSchemaTable();
                        MapColumns(schema, bcp, dest);
                    }

                    if (sqlBulkCopyCustomizer != null)
                        sqlBulkCopyCustomizer(bcp);

                    bcp.WriteToServer(reader);
                }
            }
        }

        private static void MapColumns(DataTable sourceSchema, SqlBulkCopy bcp, SqlConnection dest)
        {
            using (var cmd = dest.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0}", bcp.DestinationTableName);

                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    var destSchema = reader.GetSchemaTable();
                    reader.Close();

                    var columns = from destRow in destSchema.AsEnumerable()
                        let isReadOnly = destRow.Field<bool>("IsReadOnly")
                        let isAutoIncrement = destRow.Field<bool>("IsAutoIncrement")
                        let isIdentity = destRow.Field<bool>("IsIdentity")
                        //let isRowVersion = destRow.Field<bool>("IsRowVersion") insertable?
                        let name = destRow.Field<string>("ColumnName")
                        where !isReadOnly || isIdentity || isAutoIncrement
                        join sourceRow in sourceSchema.AsEnumerable() on name equals sourceRow.Field<string>("ColumnName")
                        select name;

                    foreach (var column in columns)
                        bcp.ColumnMappings.Add(column, column);
                }
            }

        }
    }
}