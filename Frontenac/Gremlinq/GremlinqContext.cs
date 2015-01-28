using System;
using System.Runtime.Remoting.Messaging;

namespace Frontenac.Gremlinq
{
    public class GremlinqContext
    {
        static readonly string LocalStorageSlotName = typeof (GremlinqContext).Name;

        public GremlinqContext(ITypeProvider typeProvider, IProxyFactory proxyFactory)
        {
            TypeProvider = typeProvider;
            ProxyFactory = proxyFactory;
        }

        public ITypeProvider TypeProvider { get; private set; }
        public IProxyFactory ProxyFactory { get; private set; }

        static GremlinqContext()
        {
            ContextFactory = new DefaultGremlinqContextFactory();
        }

        public static IGremlinqContextFactory ContextFactory
        {
            get { return _factory; }
            set
            {
                CallContext.FreeNamedDataSlot(LocalStorageSlotName);
                _factory = value;
            }
        }

        private static IGremlinqContextFactory _factory;

        public static GremlinqContext Current
        {
            get
            {
                var context = CallContext.LogicalGetData(LocalStorageSlotName) as GremlinqContext;
                if (context != null)
                    return context;

                context = ContextFactory.Create();
                CallContext.LogicalSetData(LocalStorageSlotName, context);
                return context;
            }
        }
    }
}