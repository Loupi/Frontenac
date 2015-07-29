using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure
{
    public class DefaultGraphFactory : IGraphFactory
    {
        private readonly IContainer _container;
        private readonly object _syncRoot = new object();

        private readonly List<IGraph> _graphs = new List<IGraph>(); 

        public DefaultGraphFactory(IContainer container)
        {
            Contract.Requires(container != null);

            _container = container;
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DefaultGraphFactory()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var graph in _graphs.ToArray())
                {
                    graph.Shutdown();
                }
            }

            _disposed = true;
        }

        #endregion

        public TGraph Create<TGraph>() where TGraph : IGraph
        {
            var graph = _container.Resolve<TGraph>();
            lock (_syncRoot)
            {
                _graphs.Add(graph);
            }
            return graph;
        }

        public void Destroy(IGraph graph)
        {
            _container.Release(graph);
            lock (_syncRoot)
            {
                _graphs.Remove(graph);
            }
        }
    }
}