using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionGraph : IGraph, IWrapperGraph
    {
        protected IGraph BaseGraph;
        string _writePartition;
        readonly ISet<string> _readPartitions;
        string _partitionKey;
        readonly Features _features;

        #region IDisposable members
        bool _disposed;

        ~PartitionGraph()
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
                BaseGraph.Dispose();
            }

            _disposed = true;
        }
        #endregion

        public PartitionGraph(IGraph baseGraph, string partitionKey, string writePartition, IEnumerable<string> readPartitions)
        {
            BaseGraph = baseGraph;
            _partitionKey = partitionKey;
            _writePartition = writePartition;
            _readPartitions = new HashSet<string>(readPartitions);
            _features = BaseGraph.GetFeatures().CopyFeatures();
            _features.IsWrapper = true;
        }

        public PartitionGraph(IGraph baseGraph, string partitionKey, string readWritePartition) :
            this(baseGraph, partitionKey, readWritePartition, new[] { readWritePartition })
        {

        }

        public string GetWritePartition()
        {
            return _writePartition;
        }

        public void SetWritePartition(string writePartition)
        {
            _writePartition = writePartition;
        }

        public ISet<string> GetReadPartitions()
        {
            return new HashSet<string>(_readPartitions);
        }

        public void RemoveReadPartition(string readPartition)
        {
            _readPartitions.Remove(readPartition);
        }

        public void AddReadPartition(string readPartition)
        {
            _readPartitions.Add(readPartition);
        }

        public void SetPartitionKey(string partitionKey)
        {
            _partitionKey = partitionKey;
        }

        public string GetPartitionKey()
        {
            return _partitionKey;
        }

        public bool IsInPartition(IElement element)
        {
            string writePartition;
            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                writePartition = partitionElement.GetPartition();
            else
                writePartition = (string)element.GetProperty(_partitionKey);
            return (null == writePartition || _readPartitions.Contains(writePartition));
        }

        public IVertex AddVertex(object id)
        {
            var vertex = new PartitionVertex(BaseGraph.AddVertex(id), this);
            vertex.SetPartition(_writePartition);
            return vertex;
        }

        public IVertex GetVertex(object id)
        {
            IVertex vertex = BaseGraph.GetVertex(id);
            if (null == vertex || !IsInPartition(vertex))
                return null;

            return new PartitionVertex(vertex, this);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new PartitionVertexIterable(BaseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            return new PartitionVertexIterable(BaseGraph.GetVertices(key, value), this);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            var edge = new PartitionEdge(BaseGraph.AddEdge(id, ((PartitionVertex)outVertex).GetBaseVertex(), ((PartitionVertex)inVertex).GetBaseVertex(), label), this);
            edge.SetPartition(_writePartition);
            return edge;
        }

        public IEdge GetEdge(object id)
        {
            IEdge edge = BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new PartitionEdge(edge, this);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new PartitionEdgeIterable(BaseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new PartitionEdgeIterable(BaseGraph.GetEdges(key, value), this);
        }

        public void RemoveEdge(IEdge edge)
        {
            BaseGraph.RemoveEdge(((PartitionEdge)edge).GetBaseEdge());
        }

        public void RemoveVertex(IVertex vertex)
        {
            BaseGraph.RemoveVertex(((PartitionVertex)vertex).GetBaseVertex());
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }

        public Features GetFeatures()
        {
            return _features;
        }

        public IGraphQuery Query()
        {
            return new WrappedGraphQuery(BaseGraph.Query(),
                t => new PartitionEdgeIterable(t.Edges(), this),
                t => new PartitionVertexIterable(t.Vertices(), this));
        }
    }
}
