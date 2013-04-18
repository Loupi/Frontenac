using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionGraph : Graph, WrapperGraph
    {
        protected Graph baseGraph;
        string _writePartition;
        readonly ISet<string> _readPartitions;
        string _partitionKey;
        readonly Features _features;

        public PartitionGraph(Graph baseGraph, string partitionKey, string writePartition, IEnumerable<string> readPartitions)
        {
            this.baseGraph = baseGraph;
            _partitionKey = partitionKey;
            _writePartition = writePartition;
            _readPartitions = new HashSet<string>(readPartitions);
            _features = this.baseGraph.getFeatures().copyFeatures();
            _features.isWrapper = true;
        }

        public PartitionGraph(Graph baseGraph, string partitionKey, string readWritePartition) :
            this(baseGraph, partitionKey, readWritePartition, new string[] { readWritePartition })
        {

        }

        public string getWritePartition()
        {
            return _writePartition;
        }

        public void setWritePartition(string writePartition)
        {
            _writePartition = writePartition;
        }

        public ISet<string> getReadPartitions()
        {
            return new HashSet<string>(_readPartitions);
        }

        public void removeReadPartition(string readPartition)
        {
            _readPartitions.Remove(readPartition);
        }

        public void addReadPartition(string readPartition)
        {
            _readPartitions.Add(readPartition);
        }

        public void setPartitionKey(string partitionKey)
        {
            _partitionKey = partitionKey;
        }

        public string getPartitionKey()
        {
            return _partitionKey;
        }

        public bool isInPartition(Element element)
        {
            string writePartition;
            if (element is PartitionElement)
                writePartition = ((PartitionElement)element).getPartition();
            else
                writePartition = (string)element.getProperty(_partitionKey);
            return (null == writePartition || _readPartitions.Contains(writePartition));
        }

        public void shutdown()
        {
            baseGraph.shutdown();
        }

        public Vertex addVertex(object id)
        {
            PartitionVertex vertex = new PartitionVertex(baseGraph.addVertex(id), this);
            vertex.setPartition(_writePartition);
            return vertex;
        }

        public Vertex getVertex(object id)
        {
            Vertex vertex = baseGraph.getVertex(id);
            if (null == vertex || !isInPartition(vertex))
                return null;

            return new PartitionVertex(vertex, this);
        }

        public IEnumerable<Vertex> getVertices()
        {
            return new PartitionVertexIterable(baseGraph.getVertices(), this);
        }

        public IEnumerable<Vertex> getVertices(string key, object value)
        {
            return new PartitionVertexIterable(baseGraph.getVertices(key, value), this);
        }

        public Edge addEdge(object id, Vertex outVertex, Vertex inVertex, string label)
        {
            PartitionEdge edge = new PartitionEdge(baseGraph.addEdge(id, ((PartitionVertex)outVertex).getBaseVertex(), ((PartitionVertex)inVertex).getBaseVertex(), label), this);
            edge.setPartition(_writePartition);
            return edge;
        }

        public Edge getEdge(object id)
        {
            Edge edge = baseGraph.getEdge(id);
            if (null == edge)
                return null;

            return new PartitionEdge(edge, this);
        }

        public IEnumerable<Edge> getEdges()
        {
            return new PartitionEdgeIterable(baseGraph.getEdges(), this);
        }

        public IEnumerable<Edge> getEdges(string key, object value)
        {
            return new PartitionEdgeIterable(baseGraph.getEdges(key, value), this);
        }

        public void removeEdge(Edge edge)
        {
            baseGraph.removeEdge(((PartitionEdge)edge).getBaseEdge());
        }

        public void removeVertex(Vertex vertex)
        {
            baseGraph.removeVertex(((PartitionVertex)vertex).getBaseVertex());
        }

        public Graph getBaseGraph()
        {
            return baseGraph;
        }

        public override string ToString()
        {
            return StringFactory.graphString(this, baseGraph.ToString());
        }

        public Features getFeatures()
        {
            return _features;
        }

        public GraphQuery query()
        {
            return new WrappedGraphQuery(baseGraph.query(),
                t => new PartitionEdgeIterable(t.edges(), this),
                t => new PartitionVertexIterable(t.vertices(), this));
        }
    }
}
