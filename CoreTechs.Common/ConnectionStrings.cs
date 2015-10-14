using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CoreTechs.Common
{
    public class ConnectionStrings
    {
        public static ConnectionStringSettings Default => GetConnectionString();

        public static ConnectionStringSettings GetConnectionString([CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            return ConfigurationManager.ConnectionStrings[name];
        }
    }
}