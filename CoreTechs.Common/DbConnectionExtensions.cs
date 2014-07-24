using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    public static class DbConnectionExtensions
    {

        public static dynamic AsDynamic(this DataRow row)
        {
            if (row == null) throw new ArgumentNullException("row");
            return new DynamicDataRow(row);
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
        public static T Scalar<T>(this DbConnection conn, string sql, CommandType commandType, DbParameter[] parameters)
        {
            using (var dataset = Query(conn, sql, commandType, parameters))
                return GetScalar<T>(sql, commandType, parameters, dataset);
        }

        /// <summary>
        /// Executes the sql query and returns the first value in the first row of the result.
        /// </summary>
        /// <exception cref="DataException">Thrown if no rows are returned for the query.</exception>
        public static async Task<T> ScalarAsync<T>(this DbConnection conn, string sql, CommandType commandType,
            DbParameter[] parameters)
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
        public static DataSet Query( this DbConnection conn,  string sql, CommandType commandType, params DbParameter[] parameters)
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
        public static async Task<DataSet> QueryAsync( this DbConnection conn,  string sql, CommandType commandType,
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
    }
}