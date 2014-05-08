using System.Configuration;
using System.Runtime.CompilerServices;

namespace CoreTechs.Common
{
    public class ConnectionStrings
    {
        public static ConnectionStringSettings Default { get { return GetConnectionString(); } }

        public static ConnectionStringSettings GetConnectionString([CallerMemberName] string name = null)
        {
            return ConfigurationManager.ConnectionStrings[name];
        }
    }
}