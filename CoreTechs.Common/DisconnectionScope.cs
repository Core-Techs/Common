using System;
using System.Data;

namespace CoreTechs.Common
{
    /// <summary>
    /// Closes the connection on construction if it's not already closed.
    /// On disposal, the connection will be opened if it was open before construction.
    /// </summary>
    public class DisconnectionScope : IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly ConnectionState _initState;

        public DisconnectionScope(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = connection;
            _initState = connection.State;

            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }

        public void Dispose()
        {
            if (_initState == ConnectionState.Open && _connection.State != ConnectionState.Open)
                _connection.Open();
        }
    }
}