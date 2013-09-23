using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public abstract class PartitionElement : DictionaryElement
    {
        protected readonly IElement BaseElement;
        protected PartitionGraph Graph;

        protected PartitionElement(IElement baseElement, PartitionGraph partitionGraph)
        {
            Contract.Requires(baseElement != null);
            Contract.Requires(partitionGraph != null);

            BaseElement = baseElement;
            Graph = partitionGraph;
        }

        public override void SetProperty(string key, object value)
        {
            if (!key.Equals(Graph.PartitionKey))
                BaseElement.SetProperty(key, value);
        }

        public override object GetProperty(string key)
        {
            return key.Equals(Graph.PartitionKey) ? null : BaseElement.GetProperty(key);
        }

        public override object RemoveProperty(string key)
        {
            return key.Equals(Graph.PartitionKey) ? null : BaseElement.RemoveProperty(key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            var keys = new HashSet<string>(BaseElement.GetPropertyKeys());
            keys.Remove(Graph.PartitionKey);
            return keys;
        }

        public override object Id
        {
            get { return BaseElement.Id; }
        }

        public override void Remove()
        {
            if (this is IVertex)
                Graph.RemoveVertex(this as IVertex);
            else
                Graph.RemoveEdge(this as IEdge);
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
            Contract.Ensures(Contract.Result<IElement>() != null);
            return BaseElement;
        }

        public string GetPartition()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
            return (string) BaseElement.GetProperty(Graph.PartitionKey);
        }

        public void SetPartition(string partition)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(partition));
            BaseElement.SetProperty(Graph.PartitionKey, partition);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}