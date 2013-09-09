using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionEdgeIterable : ICloseableIterable<IEdge>
    {
        readonly IEnumerable<IEdge> _iterable;
        readonly PartitionGraph _graph;
        bool _disposed;

        public PartitionEdgeIterable(IEnumerable<IEdge> iterable, PartitionGraph graph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(graph != null);

            _iterable = iterable;
            _graph = graph;
        }

        ~PartitionEdgeIterable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_iterable is IDisposable)
                    (_iterable as IDisposable).Dispose();
            }

            _disposed = true;
        }

        public IEnumerator<IEdge> GetEnumerator()
        {
            return new InnerPartitionEdgeIterable(this).GetEnumerator();
        }

        class InnerPartitionEdgeIterable : IEnumerable<IEdge>
        {
            readonly PartitionEdgeIterable _partitionEdgeIterable;
            readonly IEnumerator<IEdge> _itty;
            PartitionEdge _nextEdge;

            public InnerPartitionEdgeIterable(PartitionEdgeIterable partitionEdgeIterable)
            {
                Contract.Requires(partitionEdgeIterable != null);

                _partitionEdgeIterable = partitionEdgeIterable;
                _itty = _partitionEdgeIterable._iterable.GetEnumerator();
            }

            public IEnumerator<IEdge> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _nextEdge)
                    {
                        var temp = _nextEdge;
                        _nextEdge = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            var edge = _itty.Current;
                            if (_partitionEdgeIterable._graph.IsInPartition(edge))
                                yield return new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        }
                    }
                }
            }

            bool HasNext()
            {
                if (null != _nextEdge)
                    return true;

                while (_itty.MoveNext())
                {
                    var edge = _itty.Current;
                    if (_partitionEdgeIterable._graph.IsInPartition(edge))
                    {
                        _nextEdge = new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        return true;
                    }
                }
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IEdge>).GetEnumerator();
        }
    }
}
