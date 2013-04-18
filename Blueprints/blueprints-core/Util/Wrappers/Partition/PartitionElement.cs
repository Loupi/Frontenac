using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public abstract class PartitionElement : Element
    {
        protected Element baseElement;
        protected PartitionGraph graph;

        protected PartitionElement(Element baseElement, PartitionGraph partitionGraph)
        {
            this.baseElement = baseElement;
            graph = partitionGraph;
        }

        public void setProperty(string key, object value)
        {
            if (!key.Equals(graph.getPartitionKey()))
                baseElement.setProperty(key, value);
        }

        public object getProperty(string key)
        {
            if (key.Equals(graph.getPartitionKey()))
                return null;

            return baseElement.getProperty(key);
        }

        public object removeProperty(string key)
        {
            if (key.Equals(graph.getPartitionKey()))
                return null;

            return baseElement.removeProperty(key);
        }

        public IEnumerable<string> getPropertyKeys()
        {
            ISet<string> keys = new HashSet<string>(baseElement.getPropertyKeys());
            keys.Remove(graph.getPartitionKey());
            return keys;
        }

        public object getId()
        {
            return baseElement.getId();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.areEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return baseElement.GetHashCode();
        }

        public Element getBaseElement()
        {
            return baseElement;
        }

        public string getPartition()
        {
            return (string)baseElement.getProperty(graph.getPartitionKey());
        }

        public void setPartition(string partition)
        {
            baseElement.setProperty(graph.getPartitionKey(), partition);
        }

        public void remove()
        {
            if (this is Vertex)
                graph.removeVertex(this as Vertex);
            else
                graph.removeEdge(this as Edge);
        }

        public override string ToString()
        {
            return baseElement.ToString();
        }
    }
}
