using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CoreTechs.Common.Reflection;
using System.Data.Entity;

namespace CoreTechs.Common.Database
{
    public static class Extensions
    {
        public static Task OpenAsync(this IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var dbConn = connection as DbConnection;

            if (dbConn == null)
                throw new NotSupportedException("OpenAsync not supported for type: " + connection.GetType().FullName);

            return dbConn.OpenAsync(cancellationToken);
        }

        public static Task ExecuteNonQueryAsync(this IDbCommand command, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (command == null) throw new ArgumentNullException("command");

            var dbCmd = command as DbCommand;
            if (dbCmd == null)
                throw new NotSupportedException("ExecuteNonQueryAsync not supported for type: " + command.GetType().FullName);

            return dbCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public static Task<DbDataReader> ExecuteReaderAsync(this IDbCommand command, CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (command == null) throw new ArgumentNullException("command");

            var dbCmd = command as DbCommand;
            if (dbCmd == null)
                throw new NotSupportedException("ExecuteReaderAsync not supported for type: " + command.GetType().FullName);

            return dbCmd.ExecuteReaderAsync(commandBehavior, cancellationToken);
        }

        public static Task<object> ExecuteScalarAsync(this IDbCommand command, CancellationToken cancellationToken = default (CancellationToken))
        {
            if (command == null) throw new ArgumentNullException("command");

            var dbCmd = command as DbCommand;
            if (dbCmd == null)
                throw new NotSupportedException("ExecuteScalarAsync not supported for type: " + command.GetType().FullName);

            return dbCmd.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new instance of T using the default constructor and then maps values from row's columns
        /// to the instance's properties where the names and types match.
        /// 
        /// If you don't have or don't want to use a default constructor, create the objects yourself and
        /// use the <see cref="Map{T}"/> extension method instead.
        /// </summary>
        public static T Create<T>(this DataRow row) where T : class, new()
        {
            var obj = new T();
            Map(row, obj);
            return obj;
        }

        /// <summary>
        /// Sets the properties of the instance using the values of row column's where the names and types match.
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

                if (value == DBNull.Value)
                    value = null;

                if (!prop.PropertyType.IsAssignableFrom(col.DataType))
                    value = value.ConvertTo(prop.PropertyType);

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
        public static ConnectionScope Connect(this IDbConnection connection)
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
        public static async Task<ConnectionScope> ConnectAsync(this IDbConnection connection, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var scope = new ConnectionScope(connection);
            await scope.OpenAsync(cancellationToken);
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
        public static T ScalarSql<T>(this IDbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return Scalar<T>(conn, sql, CommandType.Text, null, null, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static Task<T> ScalarSqlAsync<T>(this IDbConnection conn, string sql,
            params DbParameter[] parameters)
        {
            return ScalarSqlAsync<T>(conn, sql, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static Task<T> ScalarSqlAsync<T>(this IDbConnection conn, string sql, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return ScalarAsync<T>(conn, sql, CommandType.Text, null, null, cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static T ScalarProc<T>(this IDbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            return Scalar<T>(conn, procedureName, CommandType.StoredProcedure, null, null, parameters);
        }


        /// <summary>
        /// Executes the stored procedure and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static Task<T> ScalarProcAsync<T>(this IDbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            return ScalarProcAsync<T>(conn, procedureName, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static Task<T> ScalarProcAsync<T>(this IDbConnection conn, string procedureName, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            return ScalarAsync<T>(conn, procedureName, CommandType.StoredProcedure, null, null, cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static T Scalar<T>(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout, IDbTransaction transaction,
            params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout, transaction, parameters))
            using (conn.Connect())
            {
                var scalar = cmd.ExecuteScalar();
                var converted = scalar.ConvertTo<T>();
                return converted;
            }
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static Task<T> ScalarAsync<T>(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout, IDbTransaction transaction,
            params DbParameter[] parameters)
        {
            return conn.ScalarAsync<T>(sql, commandType, commandTimeout, transaction, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static async Task<T> ScalarAsync<T>(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout, IDbTransaction transaction, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout, transaction, parameters))
            using (await conn.ConnectAsync(cancellationToken))
            {
                var scalar = await cmd.ExecuteScalarAsync(cancellationToken);
                var converted = scalar.ConvertTo<T>();
                return converted;
            }
        }

        /// <summary>
        /// Executes the sql query and returns all result sets.
        /// </summary>
        public static DataSet QuerySql(this IDbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return Query(conn, sql, CommandType.Text, CommandBehavior.Default, null,null, parameters);
        }


        /// <summary>
        /// Executes the sql query and returns all result sets.
        /// </summary>
        public static Task<DataSet> QuerySqlAsync(this IDbConnection conn, string sql,
            params DbParameter[] parameters)
        {
            return QuerySqlAsync(conn, sql, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the sql query and returns all result sets.
        /// </summary>
        public static Task<DataSet> QuerySqlAsync(this IDbConnection conn, string sql, CancellationToken cancellationToken,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            return QueryAsync(conn, sql, CommandType.Text, CommandBehavior.Default, null,null, cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns all result sets.
        /// </summary>
        public static DataSet QueryProc(this IDbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");
            return Query(conn, procedureName, CommandType.StoredProcedure, CommandBehavior.Default, null,null, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns all result sets.
        /// </summary>
        public static Task<DataSet> QueryProcAsync(this IDbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            return QueryProcAsync(conn, procedureName, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the stored procedure and returns all result sets.
        /// </summary>
        public static Task<DataSet> QueryProcAsync(this IDbConnection conn, string procedureName, CancellationToken cancellationToken,
            params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");
            return QueryAsync(conn, procedureName, CommandType.StoredProcedure, CommandBehavior.Default, null, null,
                cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the sql and returns all result sets.
        /// </summary>
        public static DataSet Query(this IDbConnection conn, string sql, CommandType commandType, CommandBehavior commandBehavior, TimeSpan? commandTimeout, IDbTransaction transaction, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            var dataset = new DataSet { EnforceConstraints = false };
            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout, transaction, parameters))
            using (conn.Connect())
            using (var reader = cmd.ExecuteReader(commandBehavior))
            {
                dataset.Load(reader);
                reader.Close();
            }

            return dataset;
        }

        /// <summary>
        /// Executes the sql and returns all result sets.
        /// </summary>
        public static Task<DataSet> QueryAsync(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout, IDbTransaction transaction, params DbParameter[] parameters)
        {
            return QueryAsync(conn, sql, commandType, CommandBehavior.Default, commandTimeout, transaction,
                CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the sql and returns all result sets.
        /// </summary>
        public static async Task<DataSet> QueryAsync(this IDbConnection conn, string sql, CommandType commandType, CommandBehavior commandBehavior, TimeSpan? commandTimeout, IDbTransaction transaction, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            var dataset = new DataSet { EnforceConstraints = false };

            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout, transaction, parameters))
            using (await conn.ConnectAsync(cancellationToken))
            using (var reader = await cmd.ExecuteReaderAsync(commandBehavior, cancellationToken))
            {
                dataset.Load(reader);
                reader.Close();
            }

            return dataset;
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static void ExecuteSql(this IDbConnection conn, string sql, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            Execute(conn, sql, CommandType.Text, null,null, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static async Task ExecuteSqlAsync(this IDbConnection conn, string sql, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (sql == null) throw new ArgumentNullException("sql");

            await ExecuteAsync(conn, sql, CommandType.Text, null,null, cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static Task ExecuteSqlAsync(this IDbConnection conn, string sql, params DbParameter[] parameters)
        {
            return ExecuteSqlAsync(conn, sql, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        public static void ExecuteProc(this IDbConnection conn, string procedureName, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            Execute(conn, procedureName, CommandType.StoredProcedure, null,null, parameters);
        }

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        public static Task ExecuteProcAsync(this IDbConnection conn, string procedureName,
            params DbParameter[] parameters)
        {
            return ExecuteProcAsync(conn, procedureName, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        public static Task ExecuteProcAsync(this IDbConnection conn, string procedureName, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            if (conn == null) throw new ArgumentNullException("conn");
            if (procedureName == null) throw new ArgumentNullException("procedureName");

            return ExecuteAsync(conn, procedureName, CommandType.StoredProcedure, null,null, cancellationToken, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static void Execute(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout,IDbTransaction transaction,
            params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout,transaction, parameters))
            using (conn.Connect())
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static Task ExecuteAsync(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout,IDbTransaction transaction,
            params DbParameter[] parameters)
        {
            return conn.ExecuteAsync(sql, commandType, commandTimeout, transaction, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Executes the sql.
        /// </summary>
        public static async Task ExecuteAsync(this IDbConnection conn, string sql, CommandType commandType, TimeSpan? commandTimeout,IDbTransaction  transaction, CancellationToken cancellationToken, params DbParameter[] parameters)
        {
            using (var cmd = CreateCommand(conn, sql, commandType, commandTimeout,transaction, parameters))
            using (await conn.ConnectAsync(cancellationToken))
                await cmd.ExecuteNonQueryAsync(cancellationToken);
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

        private static IDbCommand CreateCommand(IDbConnection conn, string sql, CommandType cmdType, TimeSpan? commandTimeout, IDbTransaction transaction,
            params DbParameter[] parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandType = cmdType;
            cmd.CommandText = sql;

            if (transaction != null)
                cmd.Transaction = transaction;

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = (int)commandTimeout.Value.TotalSeconds;

            if (parameters != null)
                foreach (var dbParameter in parameters)
                    cmd.Parameters.Add(dbParameter);

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
                dataset.Tables.Add(table); // add before load so dataset options take effect
                table.Load(dataReader);

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
        /// Yields all rows from all tables in the dataset.
        /// This is mostly useful when you have a dataset known to have a single table.
        /// </summary>
        public static IEnumerable<dynamic> AsDynamic(this DataSet dataset)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            return dataset.AsEnumerable().Select(x => x.AsDynamic());
        }

        /// <summary>
        /// Yields all rows from all tables in the dataset.
        /// This is mostly useful when you have a dataset known to have a single table.
        /// </summary>
        public static IEnumerable<dynamic> AsDynamic(this DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException("dataTable");
            return dataTable.AsEnumerable().Select(x => x.AsDynamic());
        }

        /// <summary>
        /// Maps all rows in the table to the specified type.
        /// </summary>
        public static IEnumerable<T> AsEnumerable<T>(this DataTable dataTable) where T : class, new()
        {
            if (dataTable == null) throw new ArgumentNullException("dataTable");
            return dataTable.AsEnumerable().Select(row => row.Create<T>());
        }

        /// <summary>
        /// Maps all rows in each table in the data set to the specified type.
        /// </summary>
        public static IEnumerable<T> AsEnumerable<T>(this DataSet dataSet) where T : class, new()
        {
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            return dataSet.AsEnumerable().Select(row => row.Create<T>());
        }



        /*
         * Credit to Graham O'Neale
         * http://stackoverflow.com/questions/3642371/how-to-update-only-one-field-using-entity-framework
         */
        /// <summary>
        /// Sets each provided property to Modified. Changes are not saved to the database.
        /// </summary>       
        /// <param name="context">The database context</param>
        /// <param name="entity">The entity being updated. This entity cannot already be attached to the Entity graph.</param>
        /// <param name="properties"> A list of property names to be set to modified. </param>
        /// <returns>True if no error was thrown.</returns>
        public static bool Update<T>(this DbContext context, T entity, params string[] properties) where T : class, new()
        {
            context.Set<T>().Attach(entity);
            foreach (var property in properties)
            {
                context.Entry(entity).Property(property).IsModified = true;
            }
            return true;
        }

        /*
         * Credit to Ladislav Mrnka
         * http://stackoverflow.com/questions/5749110/readonly-properties-in-ef-4-1/5749469#5749469
         */
        /// <summary>
        /// Sets each provided property to Modified. Changes are not saved to the database.
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="entity">The entity being updated. This entity cannot already be attached to the Entity graph.</param>
        /// <param name="properties"> A list of expressions providing the properties to set as modified.</param>
        /// <returns>True if no error was thrown.</returns>
        public static bool Update<T>(this DbContext context, T entity, params Expression<Func<T, object>>[] properties) where T : class, new()
        {
            context.Set<T>().Attach(entity);
            foreach (var selector in properties)
            {
                context.Entry(entity).Property(selector).IsModified = true;
            }
            return true;
        }

    }
}
