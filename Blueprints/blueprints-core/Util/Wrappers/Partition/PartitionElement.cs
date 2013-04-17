using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public abstract class PartitionElement : Element
    {
        protected Element _BaseElement;
        protected PartitionGraph _Graph;

        protected PartitionElement(Element baseElement, PartitionGraph partitionGraph)
        {
            _BaseElement = baseElement;
            _Graph = partitionGraph;
        }

        public void SetProperty(string key, object value)
        {
            if (!key.Equals(_Graph.GetPartitionKey()))
                _BaseElement.SetProperty(key, value);
        }

        public object GetProperty(string key)
        {
            if (key.Equals(_Graph.GetPartitionKey()))
                return null;

            return _BaseElement.GetProperty(key);
        }

        public object RemoveProperty(string key)
        {
            if (key.Equals(_Graph.GetPartitionKey()))
                return null;

            return _BaseElement.RemoveProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            ISet<string> keys = new HashSet<string>(_BaseElement.GetPropertyKeys());
            keys.Remove(_Graph.GetPartitionKey());
            return keys;
        }

        public object GetId()
        {
            return _BaseElement.GetId();
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return _BaseElement.GetHashCode();
        }

        public Element GetBaseElement()
        {
            return _BaseElement;
        }

        public string GetPartition()
        {
            return (string)_BaseElement.GetProperty(_Graph.GetPartitionKey());
        }

        public void SetPartition(string partition)
        {
            _BaseElement.SetProperty(_Graph.GetPartitionKey(), partition);
        }

        public void Remove()
        {
            if (this is Vertex)
                _Graph.RemoveVertex(this as Vertex);
            else
                _Graph.RemoveEdge(this as Edge);
        }

        public override string ToString()
        {
            return _BaseElement.ToString();
        }
    }
}
