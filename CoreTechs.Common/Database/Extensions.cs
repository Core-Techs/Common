using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using CoreTechs.Common.Reflection;

namespace CoreTechs.Common.Database
{
    public static class Extensions
    {
        /// <summary>
        /// Creates a new instance of T using the default constructor and then maps values from row's columns
        /// to the instance's properties where the names and types match.
        /// </summary>
        public static T Create<T>(this DataRow row) where T : class
        {
            var obj = Activator.CreateInstance<T>();
            Map(row, obj);
            return obj;
        }

        /// <summary>
        /// Set's the properties of the instance using the values of row column's where the names and types match.
        /// </summary>
        public static void Map<T>(this DataRow row, T destination) where T : class
        {
            var pubProps = destination.GetType().GetPropertiesAsDeclared().Where(x => x.CanWrite);

            foreach (var prop in pubProps)
            {
                var col = row.GetColumn(prop.Name);

                if (col == null)
                    continue;

                var value = row[col];

                if (!prop.PropertyType.IsAssignableFrom(col.DataType))
                {
                    value = value.ConvertTo(prop.PropertyType);
                }

                prop.SetValue(destination, value == DBNull.Value ? null : value);
            }
        }

        /// <summary>
        /// Gets the DataColumn of the underlying DataTable or null if it doesn't exist.
        /// </summary>
        public static DataColumn GetColumn(this DataRow row, string columnName)
        {
            if (columnName == null) throw new ArgumentNullException("columnName");
            return row.HasColumn(columnName) ? row.Table.Columns[columnName] : null;
        }

        /// <summary>
        /// Gets the DataColumn of the underlying DataTable or null if it doesn't exist.
        /// </summary>
        public static DataColumn GetColumn(this DataRow row, int columnIndex)
        {
            return row.Table.Columns.Cast<DataColumn>().ElementAtOrDefault(columnIndex);
        }

        /// <summary>
        /// Checks for the presence of a DataColumn on the underlying data table.
        /// </summary>
        public static bool HasColumn(this DataRow row, string columnName)
        {
            if (columnName == null) throw new ArgumentNullException("columnName");
            return row.Table.Columns.Contains(columnName);
        }

        /// <summary>
        /// Checks for the presence of a DataColumn on the underlying data table.
        /// </summary>
        public static bool HasColumn(this DataRow row, int columnIndex)
        {
            return GetColumn(row, columnIndex) != null;
        }

        public static dynamic AsDynamic(this DataRow row)
        {
            if (row == null) throw new ArgumentNullException("row");
            return new DynamicDataRow(row);
        }

        public static IEnumerable<dynamic> AsDynamic(this IEnumerable<DataRow> rows)
        {
            if (rows == null) throw new ArgumentNullException("rows");
            return rows.Select(r => r.AsDynamic());
        }

        /// <summary>
        /// Creates a <see cref="ConnectionScope"/> and ensures the connection is opened.
        /// When the returned ConnectionScope is disposed, the connection will be closed
        /// if it was previously closed.
        /// </summary>
        public static ConnectionScope Connect(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var scope = new ConnectionScope(connection);
            scope.Open();
            return scope;
        }

        /// <summary>
        /// Creates a <see cref="ConnectionScope"/> and ensures the connection is opened.
        /// When the returned ConnectionScope is disposed, the connection will be closed
        /// if it was previously closed.
        /// </summary>
        public static async Task<ConnectionScope> ConnectAsync(this DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var scope = new ConnectionScope(connection);
            await scope.OpenAsync();
            return scope;
        }

        /// <summary>
        /// Creates a <see cref="DisconnectionScope"/> and ensures the connection is closed.
        /// When the returned DisconnectionScope is disposed, the connection will be opened
        /// if it was previously open.
        /// </summary>
        public static DisconnectionScope Disconnect(this IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            return new DisconnectionScope(connection);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static T ScalarSql<T>(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return Scalar<T>(conn, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        async public static Task<T> ScalarSqlAsync<T>(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return await ScalarAsync<T>(conn, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static T ScalarProc<T>(this DbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            return Scalar<T>(conn, procedureName, CommandType.StoredProcedure, parameters);
        }


        /// <summary>
        /// Executes the stored procedure and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static async Task<T> ScalarProcAsync<T>(this DbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            return await ScalarAsync<T>(conn, procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static T Scalar<T>(this DbConnection conn, string sql, CommandType commandType, params DbParameter[] parameters)
        {
            using (var dataset = Query(conn, sql, commandType, parameters))
                return GetScalar<T>(sql, commandType, parameters, dataset);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static async Task<T> ScalarAsync<T>(this DbConnection conn, string sql, CommandType commandType,
           params DbParameter[] parameters)
        {
            using (var dataset = await QueryAsync(conn, sql, commandType, parameters))
                return GetScalar<T>(sql, commandType, parameters, dataset);
        }

        /// <summary>
        /// Gets the first value from the result.
        /// </summary>
        private static T GetScalar<T>(string sql, CommandType commandType, DbParameter[] parameters, DataSet dataset)
        {
            var table = dataset.Tables[0];
            var row = table.AsEnumerable().FirstOrDefault();
            if (row != null)
                return row.Field<T>(0);

            throw new DataException(string.Format("No rows were returned. {0}", new { sql, commandType }))
                .WithData("DbParameters", parameters);
        }

        /// <summary>
        /// Executes the sql query and returns all result sets.
        /// </summary>
        public static DataSet QuerySql(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return Query(conn, sql, CommandType.Text, parameters);
        }


        /// <summary>
        /// Executes the sql query and returns all result sets.
        /// </summary>
        public static async Task<DataSet> QuerySqlAsync(this DbConnection conn, string sql,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return await QueryAsync(conn, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns all result sets.
        /// </summary>
        public static DataSet QueryProc(this DbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");
            return Query(conn, procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns all result sets.
        /// </summary>
        public static async Task<DataSet> QueryProcAsync(this DbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");
            return await QueryAsync(conn, procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Executes the sql and returns all result sets.
        /// </summary>
        public static DataSet Query(this DbConnection conn, string sql, CommandType commandType, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            var dataset = new DataSet();
            using (var cmd = CreateCommand(conn, sql, commandType, parameters))
            using (conn.Connect())
            using (var reader = cmd.ExecuteReader())
                dataset.Load(reader);

            return dataset;
        }

        /// <summary>
        /// Executes the sql and returns all result sets.
        /// </summary>
        public static async Task<DataSet> QueryAsync(this DbConnection conn, string sql, CommandType commandType,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            var dataset = new DataSet();
            using (var cmd = CreateCommand(conn, sql, commandType, parameters))
            using (await conn.ConnectAsync())
            using (var reader = await cmd.ExecuteReaderAsync())
                dataset.Load(reader);

            return dataset;
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static void ExecuteSql(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            Execute(conn, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static async Task ExecuteSqlAsync(this DbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            await ExecuteAsync(conn, sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        public static void ExecuteProc(this DbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            Execute(conn, procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        public static async Task ExecuteProcAsync(this DbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            await ExecuteAsync(conn, procedureName, CommandType.StoredProcedure, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static void Execute(this DbConnection conn, string sql, CommandType commandType, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, parameters))
            using (conn.Connect())
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public async static Task ExecuteAsync(this DbConnection conn, string sql, CommandType commandType, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, parameters))
            using (conn.ConnectAsync())
                await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Adds the key and value to the exception's Data property.
        /// </summary>
        /// <returns>The exception.</returns>
        public static T WithData<T>(this T e, object key, object value) where T : Exception
        {
            e.Data.Add(key, value);
            return e;
        }

        private static DbCommand CreateCommand(DbConnection conn, string sql, CommandType cmdType, params DbParameter[] parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandType = cmdType;
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        /// <summary>
        /// Much like DataTable.Load, this method will populate the dataset
        /// by create add a new DataTable for each result set returned by the
        /// DataReader.
        /// </summary>
        public static void Load(this DataSet dataset, IDataReader dataReader)
        {
            do
            {
                var table = new DataTable();
                table.Load(dataReader);
                dataset.Tables.Add(table);

            } while (!dataReader.IsClosed);
        }

        /// <summary>
        /// Yields all rows from all tables in the dataset.
        /// This is mostly useful when you have a dataset known to have a single table.
        /// </summary>
        public static IEnumerable<DataRow> AsEnumerable(this DataSet dataset)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            return dataset.Tables.Cast<DataTable>().SelectMany(t => t.AsEnumerable());
        }

        /// <summary>
        /// Maps all rows in the table to the specified type.
        /// </summary>
        public static IEnumerable<T> AsEnumerable<T>(this DataTable dataTable) where T : class
        {
            if (dataTable == null) throw new ArgumentNullException("dataTable");
            return dataTable.AsEnumerable().Select(row => row.Create<T>());
        }

        /// <summary>
        /// Maps all rows in each table in the data set to the specified type.
        /// </summary>
        public static IEnumerable<T> AsEnumerable<T>(this DataSet dataSet) where T : class
        {
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            return dataSet.AsEnumerable().Select(row => row.Create<T>());
        }
    }
}