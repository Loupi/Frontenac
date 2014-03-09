using System.Diagnostics.Contracts;
using Castle.Core;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Installers
{
    public class InstanceStarter : IStartable
    {
        private readonly Instance _instance;

        public InstanceStarter(Instance instance)
        {
            Contract.Requires(instance != null);

            _instance = instance;
        }

        public void Start()
        {

        }

        public void Stop()
        {
            _instance.Close();
        }
    }
}