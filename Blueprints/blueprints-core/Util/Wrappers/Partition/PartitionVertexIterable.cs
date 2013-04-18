using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Vertex> _iterable;
        readonly PartitionGraph _graph;
        bool _disposed;

        public PartitionVertexIterable(IEnumerable<Vertex> iterable, PartitionGraph graph)
        {
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

        public IEnumerator<Vertex> GetEnumerator()
        {
            return new InnerPartitionVertexIterable(this).GetEnumerator();
        }

        class InnerPartitionVertexIterable : IEnumerable<Vertex>
        {
            readonly PartitionVertexIterable _partitionVertexIterable;
            readonly IEnumerator<Vertex> _itty;
            PartitionVertex _nextVertex;

            public InnerPartitionVertexIterable(PartitionVertexIterable partitionVertexIterable)
            {
                _partitionVertexIterable = partitionVertexIterable;
                _itty = _partitionVertexIterable._iterable.GetEnumerator();
            }

            public IEnumerator<Vertex> GetEnumerator()
            {
                while (hasNext())
                {
                    if (null != _nextVertex)
                    {
                        PartitionVertex temp = _nextVertex;
                        _nextVertex = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            Vertex vertex = _itty.Current;
                            if (_partitionVertexIterable._graph.isInPartition(vertex))
                                yield return new PartitionVertex(vertex, _partitionVertexIterable._graph);
                        }
                    }
                }
            }

            bool hasNext()
            {
                if (null != _nextVertex)
                    return true;

                while (_itty.MoveNext())
                {
                    Vertex vertex = _itty.Current;
                    if (_partitionVertexIterable._graph.isInPartition(vertex))
                    {
                        _nextVertex = new PartitionVertex(vertex, _partitionVertexIterable._graph);
                        return true;
                    }
                }
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<Vertex>).GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Vertex>).GetEnumerator();
        }
    }
}
