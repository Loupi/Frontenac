using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public abstract class PartitionElement : IElement
    {
        protected readonly IElement BaseElement;
        protected PartitionGraph Graph;

        protected PartitionElement(IElement baseElement, PartitionGraph partitionGraph)
        {
            BaseElement = baseElement;
            Graph = partitionGraph;
        }

        public void SetProperty(string key, object value)
        {
            if (!key.Equals(Graph.GetPartitionKey()))
                BaseElement.SetProperty(key, value);
        }

        public object GetProperty(string key)
        {
            if (key.Equals(Graph.GetPartitionKey()))
                return null;

            return BaseElement.GetProperty(key);
        }

        public object RemoveProperty(string key)
        {
            if (key.Equals(Graph.GetPartitionKey()))
                return null;

            return BaseElement.RemoveProperty(key);
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            ISet<string> keys = new HashSet<string>(BaseElement.GetPropertyKeys());
            keys.Remove(Graph.GetPartitionKey());
            return keys;
        }

        public object Id
        {
            get { return BaseElement.Id; }
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public IElement GetBaseElement()
        {
            return BaseElement;
        }

        public string GetPartition()
        {
            return (string)BaseElement.GetProperty(Graph.GetPartitionKey());
        }

        public void SetPartition(string partition)
        {
            BaseElement.SetProperty(Graph.GetPartitionKey(), partition);
        }

        public void Remove()
        {
            if (this is IVertex)
                Graph.RemoveVertex(this as IVertex);
            else
                Graph.RemoveEdge(this as IEdge);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}
