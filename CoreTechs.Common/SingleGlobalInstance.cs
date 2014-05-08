// credit: http://stackoverflow.com/a/7810107/64334

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace CoreTechs.Common
{
    public class SingleGlobalInstance : IDisposable
    {
        public bool HasHandle = false;
        private Mutex _mutex;

        private void InitMutex(Guid appGuid)
        {
          
            var mutexId = string.Format("Global\\{{{0}}}", appGuid);
            _mutex = new Mutex(false, mutexId);

            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            _mutex.SetAccessControl(securitySettings);
        }

        public SingleGlobalInstance(int timeOut = 1):this(Assembly.GetEntryAssembly(),timeOut) { }
        public SingleGlobalInstance(Assembly assembly,int timeOut = 1):this(GetAssemblyId(assembly),timeOut) { }

        private static Guid GetAssemblyId(Assembly assembly)
        {
            var appGuid =((GuidAttribute) assembly.GetCustomAttributes(typeof (GuidAttribute), false)
                    .GetValue(0)).Value;
            return new Guid(appGuid);
        }

        public SingleGlobalInstance(Guid id, int timeOut = 1)
        {
            InitMutex(id);
            try
            {
                HasHandle = _mutex.WaitOne(timeOut <= 0 ? Timeout.Infinite : timeOut, false);

                if (!HasHandle)
                    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
            }
            catch (AbandonedMutexException)
            {
                HasHandle = true;
            }
        }


        public void Dispose()
        {
            if (_mutex == null) return;
            if (HasHandle)
                _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}