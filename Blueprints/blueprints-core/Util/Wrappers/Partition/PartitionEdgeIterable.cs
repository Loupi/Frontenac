using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionEdgeIterable : CloseableIterable<Edge>
    {
        readonly IEnumerable<Edge> _Iterable;
        readonly PartitionGraph _Graph;
        bool _Disposed;

        public PartitionEdgeIterable(IEnumerable<Edge> iterable, PartitionGraph graph)
        {
            _Iterable = iterable;
            _Graph = graph;
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
            if (_Disposed)
                return;

            if (disposing)
            {
                if (_Iterable is IDisposable)
                    (_Iterable as IDisposable).Dispose();
            }

            _Disposed = true;
        }

        public IEnumerator<Edge> GetEnumerator()
        {
            return new InnerPartitionEdgeIterable(this).GetEnumerator();
        }

        class InnerPartitionEdgeIterable : IEnumerable<Edge>
        {
            readonly PartitionEdgeIterable _PartitionEdgeIterable;
            IEnumerator<Edge> _Itty;
            PartitionEdge _NextEdge;

            public InnerPartitionEdgeIterable(PartitionEdgeIterable partitionEdgeIterable)
            {
                _PartitionEdgeIterable = partitionEdgeIterable;
                _Itty = _PartitionEdgeIterable._Iterable.GetEnumerator();
            }

            public IEnumerator<Edge> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _NextEdge)
                    {
                        PartitionEdge temp = _NextEdge;
                        _NextEdge = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_Itty.MoveNext())
                        {
                            Edge edge = _Itty.Current;
                            if (_PartitionEdgeIterable._Graph.IsInPartition(edge))
                                yield return new PartitionEdge(edge, _PartitionEdgeIterable._Graph);
                        }
                    }
                }
            }

            bool HasNext()
            {
                if (null != _NextEdge)
                    return true;

                while (_Itty.MoveNext())
                {
                    Edge edge = _Itty.Current;
                    if (_PartitionEdgeIterable._Graph.IsInPartition(edge))
                    {
                        _NextEdge = new PartitionEdge(edge, _PartitionEdgeIterable._Graph);
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
