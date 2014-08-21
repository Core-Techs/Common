using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common.Database
{
    /// <summary>
    /// Opens the connection on construction if it's not already open.
    /// On disposal, the connection will be closed if it was closed before construction.
    /// </summary>
    public class ConnectionScope : IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly ConnectionState _initState;

        public ConnectionScope(IDbConnection connection)
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

        public async Task OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (_initState == ConnectionState.Closed && _connection.State != ConnectionState.Closed)
                _connection.Close();
        }
    }
}
