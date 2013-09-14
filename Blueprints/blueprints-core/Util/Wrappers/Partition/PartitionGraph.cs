using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionGraph : IGraph, IWrapperGraph
    {
        private readonly Features _features;
        private readonly ISet<string> _readPartitions;
        protected IGraph BaseGraph;
        private string _partitionKey;
        private string _writePartition;

        public PartitionGraph(IGraph baseGraph, string partitionKey, string writePartition,
                              IEnumerable<string> readPartitions)
        {
            Contract.Requires(baseGraph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(partitionKey));
            Contract.Requires(!string.IsNullOrWhiteSpace(writePartition));
            Contract.Requires(readPartitions != null);

            BaseGraph = baseGraph;
            _partitionKey = partitionKey;
            _writePartition = writePartition;
            _readPartitions = new HashSet<string>(readPartitions);
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
        }

        public PartitionGraph(IGraph baseGraph, string partitionKey, string readWritePartition) :
            this(baseGraph, partitionKey, readWritePartition, new[] {readWritePartition})
        {
        }

        public string WritePartition
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _writePartition;
            }
            set
            {
                Contract.Requires(value != null);
                _writePartition = value;
            }
        }

        public string PartitionKey
        {
            set
            {
                Contract.Requires(value != null);
                _partitionKey = value;
            }
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _partitionKey;
            }
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
            var edge =
                new PartitionEdge(
                    BaseGraph.AddEdge(id, ((PartitionVertex) outVertex).GetBaseVertex(),
                                      ((PartitionVertex) inVertex).GetBaseVertex(), label), this);
            edge.SetPartition(_writePartition);
            return edge;
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new PartitionEdge(edge, this);
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
            BaseGraph.RemoveEdge(((PartitionEdge) edge).GetBaseEdge());
        }

        public void RemoveVertex(IVertex vertex)
        {
            BaseGraph.RemoveVertex(((PartitionVertex) vertex).GetBaseVertex());
        }

        public Features Features
        {
            get { return _features; }
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                                    t => new PartitionEdgeIterable(t.Edges(), this),
                                    t => new PartitionVertexIterable(t.Vertices(), this));
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public IEnumerable<string> GetReadPartitions()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return _readPartitions.ToArray();
        }

        public void RemoveReadPartition(string readPartition)
        {
            Contract.Requires(readPartition != null);
            _readPartitions.Remove(readPartition);
        }

        public void AddReadPartition(string readPartition)
        {
            Contract.Requires(readPartition != null);
            _readPartitions.Add(readPartition);
        }

        public bool IsInPartition(IElement element)
        {
            Contract.Requires(element != null);

            string writePartition;
            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                writePartition = partitionElement.GetPartition();
            else
                writePartition = (string) element.GetProperty(_partitionKey);
            return (null == writePartition || _readPartitions.Contains(writePartition));
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, BaseGraph.ToString());
        }
    }
}