using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionVertexIterable : ICloseableIterable<IVertex>
    {
        readonly IEnumerable<IVertex> _iterable;
        readonly PartitionGraph _graph;
        bool _disposed;

        public PartitionVertexIterable(IEnumerable<IVertex> iterable, PartitionGraph graph)
        {
            Contract.Requires(iterable != null);
            Contract.Requires(graph != null);

            _iterable = iterable;
            _graph = graph;
        }

        ~PartitionVertexIterable()
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

        public IEnumerator<IVertex> GetEnumerator()
        {
            return new InnerPartitionVertexIterable(this).GetEnumerator();
        }

        class InnerPartitionVertexIterable : IEnumerable<IVertex>
        {
            readonly PartitionVertexIterable _partitionVertexIterable;
            readonly IEnumerator<IVertex> _itty;
            PartitionVertex _nextVertex;

            public InnerPartitionVertexIterable(PartitionVertexIterable partitionVertexIterable)
            {
                Contract.Requires(partitionVertexIterable != null);

                _partitionVertexIterable = partitionVertexIterable;
                _itty = _partitionVertexIterable._iterable.GetEnumerator();
            }

            public IEnumerator<IVertex> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _nextVertex)
                    {
                        var temp = _nextVertex;
                        _nextVertex = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            var vertex = _itty.Current;
                            if (_partitionVertexIterable._graph.IsInPartition(vertex))
                                yield return new PartitionVertex(vertex, _partitionVertexIterable._graph);
                        }
                    }
                }
            }

            bool HasNext()
            {
                if (null != _nextVertex)
                    return true;

                while (_itty.MoveNext())
                {
                    var vertex = _itty.Current;
                    if (_partitionVertexIterable._graph.IsInPartition(vertex))
                    {
                        _nextVertex = new PartitionVertex(vertex, _partitionVertexIterable._graph);
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
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }
    }
}
