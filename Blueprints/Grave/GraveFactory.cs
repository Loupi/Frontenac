using System;
using System.Diagnostics.Contracts;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace Frontenac.Grave
{
    public static class GraveFactory
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

        public static GraveGraph CreateGraph()
        {
            Contract.Ensures(Contract.Result<GraveGraph>() != null);

            return Context.GraphFactory.Create();
        }

        public static GraveGraph CreateTinkerGraph()
        {
            Contract.Ensures(Contract.Result<GraveGraph>() != null);

            var graph = Context.GraphFactory.Create();

            var marko = graph.AddVertex("1");
            marko.SetProperty("name", "marko");
            marko.SetProperty("age", 29);

            var vadas = graph.AddVertex("2");
            vadas.SetProperty("name", "vadas");
            vadas.SetProperty("age", 27);

            var lop = graph.AddVertex("3");
            lop.SetProperty("name", "lop");
            lop.SetProperty("lang", "java");

            var josh = graph.AddVertex("4");
            josh.SetProperty("name", "josh");
            josh.SetProperty("age", 32);

            var ripple = graph.AddVertex("5");
            ripple.SetProperty("name", "ripple");
            ripple.SetProperty("lang", "java");

            var peter = graph.AddVertex("6");
            peter.SetProperty("name", "peter");
            peter.SetProperty("age", 35);

            var loupi = graph.AddVertex("7");
            loupi.SetProperty("name", "loupi");
            loupi.SetProperty("age", 33);
            loupi.SetProperty("lang", "c#");

            graph.AddEdge("7", marko, vadas, "knows").SetProperty("weight", 0.5);
            graph.AddEdge("8", marko, josh, "knows").SetProperty("weight", 1.0);
            graph.AddEdge("9", marko, lop, "created").SetProperty("weight", 0.4);

            graph.AddEdge("10", josh, ripple, "created").SetProperty("weight", 1.0);
            graph.AddEdge("11", josh, lop, "created").SetProperty("weight", 0.4);

            graph.AddEdge("12", peter, lop, "created").SetProperty("weight", 0.2);

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
                _container.Install(FromAssembly.Named("Grave"));
                GraphFactory = _container.Resolve<IGraveGraphFactory>();
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
                    _container.Release(GraphFactory);
                    _container.Dispose();
                }

                _disposed = true;
            }

            #endregion

            internal IGraveGraphFactory GraphFactory { get; private set; }
        }
    }
}