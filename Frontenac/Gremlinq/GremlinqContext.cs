using System.Collections.Concurrent;
using System.Threading;

#if !NETSTANDARD
using System.Runtime.Remoting.Messaging;
#endif

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

#if NETSTANDARD
    public static class CallContext
    {
        static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();

        public static void FreeNamedDataSlot(string name)
        {

        }

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void LogicalSetData(string name, object data) =>
            state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static object LogicalGetData(string name) =>
            state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
    }
#endif
}