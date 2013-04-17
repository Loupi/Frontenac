using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionGraph : Graph, WrapperGraph
    {
        protected Graph _BaseGraph;
        string _WritePartition;
        ISet<string> _ReadPartitions;
        string _PartitionKey;
        readonly Features _Features;

        public PartitionGraph(Graph baseGraph, string partitionKey, string writePartition, IEnumerable<string> readPartitions)
        {
            _BaseGraph = baseGraph;
            _PartitionKey = partitionKey;
            _WritePartition = writePartition;
            _ReadPartitions = new HashSet<string>(readPartitions);
            _Features = _BaseGraph.GetFeatures().CopyFeatures();
            _Features.IsWrapper = true;
        }

        public PartitionGraph(Graph baseGraph, string partitionKey, string readWritePartition) :
            this(baseGraph, partitionKey, readWritePartition, new string[] { readWritePartition })
        {

        }

        public string GetWritePartition()
        {
            return _WritePartition;
        }

        public void setWritePartition(string writePartition)
        {
            _WritePartition = writePartition;
        }

        public ISet<string> GetReadPartitions()
        {
            return new HashSet<string>(_ReadPartitions);
        }

        public void RemoveReadPartition(string readPartition)
        {
            _ReadPartitions.Remove(readPartition);
        }

        public void AddReadPartition(string readPartition)
        {
            _ReadPartitions.Add(readPartition);
        }

        public void SetPartitionKey(string partitionKey)
        {
            _PartitionKey = partitionKey;
        }

        public string GetPartitionKey()
        {
            return _PartitionKey;
        }

        public bool IsInPartition(Element element)
        {
            string writePartition;
            if (element is PartitionElement)
                writePartition = ((PartitionElement)element).GetPartition();
            else
                writePartition = (string)element.GetProperty(_PartitionKey);
            return (null == writePartition || _ReadPartitions.Contains(writePartition));
        }

        public void Shutdown()
        {
            _BaseGraph.Shutdown();
        }

        public Vertex AddVertex(object id)
        {
            PartitionVertex vertex = new PartitionVertex(_BaseGraph.AddVertex(id), this);
            vertex.SetPartition(_WritePartition);
            return vertex;
        }

        public Vertex GetVertex(object id)
        {
            Vertex vertex = _BaseGraph.GetVertex(id);
            if (null == vertex || !IsInPartition(vertex))
                return null;

            return new PartitionVertex(vertex, this);
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return new PartitionVertexIterable(_BaseGraph.GetVertices(), this);
        }

        public IEnumerable<Vertex> GetVertices(string key, object value)
        {
            return new PartitionVertexIterable(_BaseGraph.GetVertices(key, value), this);
        }

        public Edge AddEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            PartitionEdge edge = new PartitionEdge(_BaseGraph.AddEdge(id, ((PartitionVertex)outVertex).GetBaseVertex(), ((PartitionVertex)inVertex).GetBaseVertex(), label), this);
            edge.SetPartition(_WritePartition);
            return edge;
        }

        public Edge GetEdge(object id)
        {
            Edge edge = _BaseGraph.GetEdge(id);
            if (null == edge)
                return null;

            return new PartitionEdge(edge, this);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return new PartitionEdgeIterable(_BaseGraph.GetEdges(), this);
        }

        public IEnumerable<Edge> GetEdges(string key, object value)
        {
            return new PartitionEdgeIterable(_BaseGraph.GetEdges(key, value), this);
        }

        public void RemoveEdge(Edge edge)
        {
            _BaseGraph.RemoveEdge(((PartitionEdge)edge).GetBaseEdge());
        }

        public void RemoveVertex(Vertex vertex)
        {
            _BaseGraph.RemoveVertex(((PartitionVertex)vertex).GetBaseVertex());
        }

        public Graph GetBaseGraph()
        {
            return _BaseGraph;
        }

        public override string ToString()
        {
            return StringFactory.GraphString(this, _BaseGraph.ToString());
        }

        public Features GetFeatures()
        {
            return _Features;
        }

        public GraphQuery Query()
        {
            return new WrappedGraphQuery(_BaseGraph.Query(),
                t => new PartitionEdgeIterable(t.Edges(), this),
                t => new PartitionVertexIterable(t.Vertices(), this));
        }
    }
}
