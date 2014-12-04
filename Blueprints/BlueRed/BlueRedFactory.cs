using System;
using System.Diagnostics.Contracts;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.Windsor;
using Castle.Windsor.Installer;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public static class BlueRedFactory
    {
        private static readonly object SyncRoot = new object();
        private static FactoryContext _context;

        private static FactoryContext Context
        {
            get
            {
                Contract.Ensures(Contract.Result<FactoryContext>() != null);

                lock (SyncRoot)
                {
                    if (_context == null)
                    {
                        _context = new FactoryContext();
                    }
                }
                return _context;
            }
        }

        public static void DeleteDb()
        {
            lock (SyncRoot)
            {
                //if (_context != null)
                {
                    var mp = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true");
                    mp.GetServer("localhost:6379").FlushDatabase();
                    mp.Dispose();
                }
            }
        }

        public static void Release()
        {
            lock (SyncRoot)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public static RedisGraph CreateGraph()
        {
            Contract.Ensures(Contract.Result<RedisGraph>() != null);

            return Context.GraphFactory.Create();
        }

        public static RedisGraph CreateTinkerGraph()
        {
            Contract.Ensures(Contract.Result<RedisGraph>() != null);

            var graph = Context.GraphFactory.Create();

            /*var marko = graph.AddVertex<IContributor>(t => { t.Name = "Marko"; t.Age = 29; });
            var vadas = graph.AddVertex<IContributor>(t => { t.Name = "Vadas"; t.Age = 27; });
            var lop = graph.AddVertex<IContributor>(t => { t.Name = "Lop"; t.Language = "Java"; });
            var josh = graph.AddVertex<IContributor>(t => { t.Name = "Josh"; t.Age = 32; });
            var ripple = graph.AddVertex<IContributor>(t => { t.Name = "Ripple"; t.Language = "Java"; });
            var peter = graph.AddVertex<IContributor>(t => { t.Name = "Peter"; t.Age = 35; });
            graph.AddVertex<IContributor>(t => { t.Name = "Loupi"; t.Age = 33; t.Language = "C#"; });

            marko.AddEdge(t => t.Knows, vadas, t => t.Weight = 0.5f);
            marko.AddEdge(t => t.Knows, josh, t => t.Weight = 1.0f);
            marko.AddEdge(t => t.Created, lop, t => t.Weight = 0.4f);

            josh.AddEdge(t => t.Created, ripple, t => t.Weight = 1.0f);
            josh.AddEdge(t => t.Created, lop, t => t.Weight = 0.4f);

            peter.AddEdge(t => t.Created, lop, t => t.Weight = 0.2f);*/

            return graph;
        }

        public class FactoryContext : IDisposable
        {
            private readonly IWindsorContainer _container;

            internal FactoryContext()
            {
                _container = new WindsorContainer();
                _container.AddFacility<StartableFacility>(f => f.DeferredStart());
                _container.AddFacility<TypedFactoryFacility>();
                _container.Install(FromAssembly.Named("Frontenac.BlueRed"));
                _container.Install(FromAssembly.Named("Frontenac.Infrastructure"));
                
                GraphFactory = _container.Resolve<IBlueRedGraphFactory>();
                Multiplexer = _container.Resolve<ConnectionMultiplexer>();
            }

            #region IDisposable

            private bool _disposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~FactoryContext()
            {
                Dispose(false);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    _container.Release(Multiplexer);
                    _container.Release(GraphFactory);
                    _container.Dispose();
                }

                _disposed = true;
            }

            #endregion

            internal ConnectionMultiplexer Multiplexer { get; private set; }
            internal IBlueRedGraphFactory GraphFactory { get; private set; }
        }
    }
}
