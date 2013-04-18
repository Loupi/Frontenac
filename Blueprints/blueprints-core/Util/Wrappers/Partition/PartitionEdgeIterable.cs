using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Edge> _iterable;
        readonly PartitionGraph _graph;
        bool _disposed;

        public PartitionEdgeIterable(IEnumerable<Edge> iterable, PartitionGraph graph)
        {
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

        public IEnumerator<Edge> GetEnumerator()
        {
            return new InnerPartitionEdgeIterable(this).GetEnumerator();
        }

        class InnerPartitionEdgeIterable : IEnumerable<Edge>
        {
            readonly PartitionEdgeIterable _partitionEdgeIterable;
            readonly IEnumerator<Edge> _itty;
            PartitionEdge _nextEdge;

            public InnerPartitionEdgeIterable(PartitionEdgeIterable partitionEdgeIterable)
            {
                _partitionEdgeIterable = partitionEdgeIterable;
                _itty = _partitionEdgeIterable._iterable.GetEnumerator();
            }

            public IEnumerator<Edge> GetEnumerator()
            {
                while (hasNext())
                {
                    if (null != _nextEdge)
                    {
                        PartitionEdge temp = _nextEdge;
                        _nextEdge = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            Edge edge = _itty.Current;
                            if (_partitionEdgeIterable._graph.isInPartition(edge))
                                yield return new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        }
                    }
                }
            }

            bool hasNext()
            {
                if (null != _nextEdge)
                    return true;

                while (_itty.MoveNext())
                {
                    Edge edge = _itty.Current;
                    if (_partitionEdgeIterable._graph.isInPartition(edge))
                    {
                        _nextEdge = new PartitionEdge(edge, _partitionEdgeIterable._graph);
                        return true;
                    }
                }
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (this as IEnumerable<Edge>).GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Edge>).GetEnumerator();
        }
    }
}
