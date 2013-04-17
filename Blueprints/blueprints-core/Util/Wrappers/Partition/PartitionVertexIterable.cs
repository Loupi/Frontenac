using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    class PartitionVertexIterable : CloseableIterable<Vertex>
    {
        readonly IEnumerable<Vertex> _Iterable;
        readonly PartitionGraph _Graph;
        bool _Disposed;

        public PartitionVertexIterable(IEnumerable<Vertex> iterable, PartitionGraph graph)
        {
            _Iterable = iterable;
            _Graph = graph;
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
            if (_Disposed)
                return;

            if (disposing)
            {
                if (_Iterable is IDisposable)
                    (_Iterable as IDisposable).Dispose();
            }

            _Disposed = true;
        }

        public IEnumerator<Vertex> GetEnumerator()
        {
            return new InnerPartitionVertexIterable(this).GetEnumerator();
        }

        class InnerPartitionVertexIterable : IEnumerable<Vertex>
        {
            readonly PartitionVertexIterable _PartitionVertexIterable;
            IEnumerator<Vertex> _Itty;
            PartitionVertex _NextVertex;

            public InnerPartitionVertexIterable(PartitionVertexIterable partitionVertexIterable)
            {
                _PartitionVertexIterable = partitionVertexIterable;
                _Itty = _PartitionVertexIterable._Iterable.GetEnumerator();
            }

            public IEnumerator<Vertex> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _NextVertex)
                    {
                        PartitionVertex temp = _NextVertex;
                        _NextVertex = null;
                        yield return temp;
                    }
                    else
                    {
                        while (_Itty.MoveNext())
                        {
                            Vertex vertex = _Itty.Current;
                            if (_PartitionVertexIterable._Graph.IsInPartition(vertex))
                                yield return new PartitionVertex(vertex, _PartitionVertexIterable._Graph);
                        }
                    }
                }
            }

            bool HasNext()
            {
                if (null != _NextVertex)
                    return true;

                while (_Itty.MoveNext())
                {
                    Vertex vertex = _Itty.Current;
                    if (_PartitionVertexIterable._Graph.IsInPartition(vertex))
                    {
                        _NextVertex = new PartitionVertex(vertex, _PartitionVertexIterable._Graph);
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
