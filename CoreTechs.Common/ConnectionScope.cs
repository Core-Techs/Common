using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    /// <summary>
    /// Opens the connection on construction if it's not already open.
    /// On disposal, the connection will be closed if it was closed before construction.
    /// </summary>
    public class ConnectionScope : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly ConnectionState _initState;

        public ConnectionScope(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
            _initState = connection.State;

        }

        public void Open()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public async Task OpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        public void Dispose()
        {
            if (_initState == ConnectionState.Closed && _connection.State != ConnectionState.Closed)
                _connection.Close();
        }
    }
}
